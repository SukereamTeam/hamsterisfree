using System.Collections;
using System.Collections.Generic;
using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Threading;
using UniRx;
using UnityEngine.UI;
using System.Linq;

public class SceneController : MonoSingleton<SceneController>
{
    private string nextScene;
    public string NextScene => this.nextScene;

    private List<UniTask> loadingTask;
    public List<UniTask> LoadingTask
    {
        get => this.loadingTask;
        private set => this.loadingTask = value;
    }
    public int TaskCount => this.loadingTask.Count;

    public int CompleteCount { get; set; }

    private bool loadingDone = false;
    public bool LoadingDone { get => this.loadingDone; set => this.loadingDone = value; }

    private AsyncOperation operation;
    public AsyncOperation Operation => this.operation;

    private Image fade = null;

    

    public void Initialize()
    {
        var prefab = Resources.Load<GameObject>("Prefabs/FadeCanvas");
        var canvas = Instantiate<GameObject>(prefab, this.transform);

        this.fade = canvas.GetComponentInChildren<Image>();

        this.loadingTask = new List<UniTask>();
    }


    public async UniTaskVoid LoadScene(Define.Scene _SceneName, bool _WithLoading)
    {
        try
        {
            var sceneString = Enum.GetName(typeof(Define.Scene), _SceneName);

            nextScene = sceneString;

            await UniTask.Yield();

            if (_WithLoading == true)
            {
                await SceneManager.LoadSceneAsync("Loading");
            }

            await UniTask.Yield();

            await UniTask.WhenAll(LoadingTask.ToArray().Select(async task =>
            {
                await task;

                Debug.Log("태스크 끝");

                CompleteCount++;
            }));

            await UniTask.Yield();

            if (_WithLoading == true)
            {
                await UniTask.WaitUntil(() => this.loadingDone == true);
            }

            await LoadSceneAsync();

            await UniTask.Yield();

            LoadingTask.Clear();
            CompleteCount = 0;
            this.loadingDone = false;

            if (this.operation != null)
            {
                this.operation = null;
            }

            this.fade.color = new Color(this.fade.color.r, this.fade.color.g, this.fade.color.b, 0f);
        }
        catch (Exception ex)
        {
            Debug.LogError($"### exception occurred: {ex}");
        }
    }


    private async UniTask LoadSceneAsync()
    {
        this.operation = SceneManager.LoadSceneAsync(nextScene);

        while (!this.operation.isDone)
        {
            await UniTask.Yield(); // 다음 프레임까지 대기
        }
    }


    public void AddLoadingTask(UniTask task)
    {
        this.loadingTask.Add(task);
    }


    // Canvas Fade In/Out -------------------------
    public async UniTask Fade(bool _FadeIn, float _Duration, bool _Skip, CancellationTokenSource cancellationToken, Action _Action = null)
    {
        var fadeInt = _FadeIn ? 1 : 0;  // fade in : 검은 화면에서 서서히 밝아지는 것!
        this.fade.color = new Color(this.fade.color.r, this.fade.color.g, this.fade.color.b, fadeInt);
        this.fade.raycastTarget = !_Skip;

        try
        {
            var negBool = !_FadeIn;
            var resultInt = negBool ? 1 : 0;

            var tweener = DOTween.To(() => this.fade.color.a, x =>
            {
                this.fade.color = new Color(this.fade.color.r, this.fade.color.g, this.fade.color.b, x);
            }, resultInt, _Duration).SetEase(Ease.OutQuint);

            Action cancelAction = () =>
            {
                Debug.Log("Cancel CanvasFade");
                tweener.Kill();

                this.fade.color = new Color(this.fade.color.r, this.fade.color.g, this.fade.color.b, 0f);

                if (_Action != null)
                    _Action();
            };

            using (cancellationToken.Token.Register(() => cancelAction()))
            {
                await tweener
                    .OnKill(() =>
                    {
                        Debug.Log("Fade was OnKill.");
                    })
                    .OnComplete(() =>
                    {
                        Debug.Log("Fade was OnComplete.");
                    })
                    .ToUniTask();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"### exception occurred: {ex}");
        }
    }

}
