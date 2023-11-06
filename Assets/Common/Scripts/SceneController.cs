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

public class SceneController : GlobalMonoSingleton<SceneController>
{
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

    [SerializeField]
    private Image fade = null;

    private CancellationTokenSource sceneCts = null;
    private CancellationTokenSource fadeCts = null;

    private const int SCENE_CHANGE_PAUSE = 500;



    private void Awake()
    {
        if (Instance == null)
        {
            _instance = this;
        }

        this.loadingTask = new List<UniTask>();

        this.sceneCts = new CancellationTokenSource();
        this.fadeCts = new CancellationTokenSource();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        this.sceneCts?.Cancel();
        this.sceneCts?.Dispose();

        this.fadeCts?.Cancel();
        this.fadeCts?.Dispose();
    }


    public async UniTaskVoid LoadScene(Define.Scene _SceneName, bool _WithLoading)
    {
        try
        {
            var sceneString = Enum.GetName(typeof(Define.Scene), _SceneName);

            await UniTask.Yield(/*cancellationToken: this.sceneCts.Token*/);

            LoadingScene loadingScene = null;
            if (_WithLoading == true)
            {
                var operation = SceneManager.LoadSceneAsync("Loading");

                while (operation != null && operation.isDone == false)
                {
                    if (this.sceneCts?.IsCancellationRequested == true)
                        return;

                    float progress = operation.progress;

                    await UniTask.Yield(/*cancellationToken: this.sceneCts.Token*/);
                }

                if (this.sceneCts.IsCancellationRequested == false)
                {
                    loadingScene = FindObjectOfType<LoadingScene>();
                    this.fade.color = new Color(this.fade.color.r, this.fade.color.g, this.fade.color.b, 0f);
                }
            }

            await UniTask.Yield(/*cancellationToken: this.sceneCts.Token*/);

            await UniTask.WhenAll(LoadingTask.ToArray().Select(async task =>
            {
                await task;

                if (loadingScene)
                {
                    CompleteCount++;
                    var amount = ((float)CompleteCount / (float)TaskCount);
                    var progress = Mathf.Round(amount * 100) / 100;    // 소수점 둘째자리까지 반올림
                    loadingScene.UpdateProgress(progress);
                }
            })).AttachExternalCancellation(this.sceneCts.Token);

            await UniTask.Yield(/*cancellationToken: this.sceneCts.Token*/);

            if (loadingScene)
            {
                // 로딩바 1f까지 다 채운 후 0.5초 쉬고 씬 이동
                loadingScene.UpdateProgress(1f);
                await UniTask.Delay(TimeSpan.FromMilliseconds(SCENE_CHANGE_PAUSE), cancellationToken: this.sceneCts.Token);
            }

            await LoadSceneAsync(sceneString, this.sceneCts);

            await UniTask.Yield(/*cancellationToken: this.sceneCts.Token*/);

            LoadingTask.Clear();
            CompleteCount = 0;
            this.loadingDone = false;

            this.fade.color = new Color(this.fade.color.r, this.fade.color.g, this.fade.color.b, 0f);
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            Debug.LogError($"### exception occurred: {ex.Message} / {ex.StackTrace}");
        }
    }


    private async UniTask LoadSceneAsync(string _SceneName, CancellationTokenSource _Cts)
    {
        var operation = SceneManager.LoadSceneAsync(_SceneName);

        while (!operation.isDone && _Cts.IsCancellationRequested == false)
        {
            await UniTask.Yield(); // 다음 프레임까지 대기
        }
    }


    public void AddLoadingTask(UniTask task)
    {
        this.loadingTask.Add(task);
    }


    // Canvas Fade In/Out -------------------------
    public async UniTask Fade(bool _FadeIn, float _Duration, bool _Skip, CancellationTokenSource _Cts = null, Action _Action = null)
    {
        var fadeInt = _FadeIn ? 1 : 0;  // fade in : 검은 화면에서 서서히 밝아지는 것!
        this.fade.color = new Color(this.fade.color.r, this.fade.color.g, this.fade.color.b, fadeInt);
        this.fade.raycastTarget = !_Skip;

        _Cts ??= this.fadeCts;

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

                if (this.fade != null)
                {
                    this.fade.color = new Color(this.fade.color.r, this.fade.color.g, this.fade.color.b, 0f);
                }

                if (_Action != null)
                    _Action();
            };

            using (_Cts.Token.Register(() => cancelAction()))
            {
                await tweener
                    .OnKill(() =>
                    {
                        Debug.Log("Fade was OnKill.");

                        if (this.fade != null)
                        {
                            this.fade.raycastTarget = false;
                        }
                    })
                    .OnComplete(() =>
                    {
                        Debug.Log("Fade was OnComplete.");
                    }).ToUniTask();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"### exception occurred: {ex.Message} / {ex.StackTrace}");
        }
    }

}
