using System.Collections;
using System.Collections.Generic;
using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Threading;
using UniRx;

public class SceneController : Singleton<SceneController>
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

    public async UniTask CanvasFadeIn(CanvasGroup _CanvasGroup, float _Duration, CancellationTokenSource cancellationToken)
    {
        _CanvasGroup.alpha = 0f;
        _CanvasGroup.interactable = false;

        try
        {
            var tweener = DOTween.To(() => _CanvasGroup.alpha, x => _CanvasGroup.alpha = x, 1f, _Duration)
                .SetEase(Ease.OutQuint);

            Action myAction = () =>
            {
                Debug.Log("Cancel CanvasFadeIn");
                tweener.Kill();

                _CanvasGroup.alpha = 1f;
            };

            using (cancellationToken.Token.Register(() => myAction()))
            {
                await tweener
                    .OnKill(() =>
                    {
                        Debug.Log("FadeIn was OnKill.");
                    })
                    .OnComplete(() =>
                    {
                        Debug.Log("FadeIn was OnComplete.");
                    })
                    .ToUniTask();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"### exception occurred: {ex}");
        }
    }

    public async UniTask CanvasFadeOut(CanvasGroup _CanvasGroup, float _Duration, CancellationTokenSource cancellationToken)
    {
        _CanvasGroup.alpha = 1f;
        _CanvasGroup.interactable = false;

        try
        {
            var tweener = DOTween.To(() => _CanvasGroup.alpha, x => _CanvasGroup.alpha = x, 0f, _Duration)
                .SetEase(Ease.OutQuint);

            Action myAction = () =>
            {
                Debug.Log("Cancel CanvasFadeOut");
                tweener.Kill();

                _CanvasGroup.alpha = 1f;
            };

            using (cancellationToken.Token.Register(() => myAction()))
            {
                await tweener
                    .OnKill(() =>
                    {
                        Debug.Log("FadeOut was OnKill.");
                    })
                    .OnComplete(() =>
                    {
                        Debug.Log("FadeOut was OnComplete.");
                    })
                    .ToUniTask();
            };
        }
        catch (Exception ex)
        {
            Debug.LogError($"### exception occurred: {ex}");
        }
    }

    // TODO
    // CanvasFadeIn/Out 부르는 애들마다
    // Cancel에서 원하는 행동이 있을 텐데
    // 이걸 Action? 으로 넣어주기?

    public void AddLoadingTask(UniTask task)
    {
        this.loadingTask.Add(task);
    }
}
