using System.Collections;
using System.Collections.Generic;
using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading;

public class SceneController
{

    private static string nextScene;

    public static List<UniTask> LoadingTask = new List<UniTask>();
    public static AsyncOperation Operation;






    public static async UniTaskVoid LoadScene(Define.Scene _SceneName)
    {
        var sceneString = Enum.GetName(typeof(Define.Scene), _SceneName);

        nextScene = sceneString;

        LoadingTask.Add(UniTask.Defer(LoadSceneAsync));



        await UniTask.WhenAll(LoadingTask.ToArray());//.ContinueWith(async () =>
        //{
        //    LoadingTask.Clear();
        //});

        //foreach (var task in LoadingTask)
        //{
        //    await task;
        //}

        //await UniTask.Yield();

        LoadingTask.Clear();
        if (Operation != null)
        {
            Operation = null;
        }

    }

    public static async UniTask LoadSceneWithLoading(Define.Scene _SceneName)
    {
        ////if (Operation != null)
        ////{
        ////    Operation = null;
        ////}

        var sceneString = Enum.GetName(typeof(Define.Scene), _SceneName);

        nextScene = sceneString;

        await UniTask.DelayFrame(1);

        await SceneManager.LoadSceneAsync("Loading");

        LoadingTask.Add(UniTask.Defer(LoadSceneAsync));

        await UniTask.WhenAll(LoadingTask.ToArray());

        await UniTask.Yield();

        LoadingTask.Clear();
        if (Operation != null)
        {
            Operation = null;
        }
    }


    private static async UniTask LoadSceneAsync()
    {
        if (Operation != null)
        {
            Operation = null;
        }

        Operation = SceneManager.LoadSceneAsync(nextScene);

        Operation.allowSceneActivation = false;

        while (!Operation.isDone)
        {
            await UniTask.Yield(); // 다음 프레임까지 대기
        }
    }

    public static async UniTask CanvasFadeIn(CanvasGroup _CanvasGroup, float _Duration, CancellationTokenSource cancellationToken)
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
                        // Cleanup or do something when the fade-in is complete
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

    public static async UniTask CanvasFadeOut(CanvasGroup _CanvasGroup, float _Duration, CancellationTokenSource cancellationToken)
    {
        _CanvasGroup.alpha = 1f;
        _CanvasGroup.interactable = false;

        try
        {
            var tweener = DOTween.To(() => _CanvasGroup.alpha, x => _CanvasGroup.alpha = x, 0f, _Duration)
                .SetEase(Ease.OutQuint);

            Action myAction = () =>
            {
                Debug.Log("아!!! 취소라고!!!~~~~~~~~~~~");
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
}
