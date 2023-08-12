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

public class SceneController : MonoSingleton<SceneController>
{
    private string nextScene;
    public string NextScene => this.nextScene;

    private List<UniTask> loadingTask = new List<UniTask>();
    public List<UniTask> LoadingTask
    {
        get => this.loadingTask;
        private set => this.loadingTask = value;
    }

    private AsyncOperation operation;
    public AsyncOperation Operation => this.operation;

    private Image fade = null;

    

    public void Initialize()
    {
        var prefab = Resources.Load<GameObject>("Prefabs/FadeCanvas");
        var canvas = Instantiate<GameObject>(prefab, this.transform);

        this.fade = canvas.GetComponentInChildren<Image>();
    }



    public async UniTask SceneActivation(UniTask task)
    {
        await task.ToObservable().Do(async x =>
        {
            Debug.Log("Subscribe !!!");

            await UniTask.WaitUntil(() => this.operation != null);

            if (this.operation != null)
            {
                Debug.Log("Operation !!!");
                this.operation.allowSceneActivation = true;
            }

            await UniTask.CompletedTask;

        }).Last();
    }


    public async UniTaskVoid LoadScene(Define.Scene _SceneName)
    {
        var sceneString = Enum.GetName(typeof(Define.Scene), _SceneName);

        nextScene = sceneString;

        LoadingTask.Add(UniTask.Defer(LoadSceneAsync));

        await UniTask.WhenAll(LoadingTask.ToArray());

        LoadingTask.Clear();

        if (this.operation != null)
        {
            this.operation = null;
        }

        this.fade.color = new Color(this.fade.color.r, this.fade.color.g, this.fade.color.b, 0f);
    }

    public async UniTask LoadSceneWithLoading(Define.Scene _SceneName)
    {
        var sceneString = Enum.GetName(typeof(Define.Scene), _SceneName);

        nextScene = sceneString;

        await UniTask.Yield();

        await SceneManager.LoadSceneAsync("Loading");

        LoadingTask.Add(UniTask.Defer(LoadSceneAsync));

        await UniTask.WhenAll(LoadingTask.ToArray());

        await UniTask.Yield();

        LoadingTask.Clear();

        if (this.operation != null)
        {
            this.operation = null;
        }

        this.fade.color = new Color(this.fade.color.r, this.fade.color.g, this.fade.color.b, 0f);
    }


    private async UniTask LoadSceneAsync()
    {
        this.operation = SceneManager.LoadSceneAsync(nextScene);

        this.operation.allowSceneActivation = false;

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
        //if (this.fade.gameObject.activeSelf == false)
        //    this.fade.gameObject.SetActive(true);

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
