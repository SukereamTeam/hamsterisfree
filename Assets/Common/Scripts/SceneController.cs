using System.Collections;
using System.Collections.Generic;
using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


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

        //await UniTask.WhenAll(LoadingTask.ToArray());

        foreach(var task in LoadingTask)
        {
            await task;
        }

        LoadingTask.Clear();
    }

    public static async UniTask LoadSceneWithLoading(Define.Scene _SceneName)
    {
        var sceneString = Enum.GetName(typeof(Define.Scene), _SceneName);

        nextScene = sceneString;

        await SceneManager.LoadSceneAsync("Loading");

        LoadingTask.Add(UniTask.Defer(LoadSceneAsync));

        await UniTask.WhenAll(LoadingTask.ToArray());

        LoadingTask.Clear();
    }


    private static async UniTask LoadSceneAsync()
    {
        if (Operation != null)
        {
            Operation = null;
        }

        Operation = SceneManager.LoadSceneAsync(nextScene);

        while (!Operation.isDone)
        {
            float progress = Mathf.Clamp01(Operation.progress / 0.9f); // allowSceneActivation이 false일 때까지 진행률을 0.9까지 제한합니다.

            if (progress >= 0.9f)
            {
                break;
            }

            await UniTask.Yield(); // 다음 프레임까지 대기
        }

        //Operation.allowSceneActivation = true;
        await UniTask.CompletedTask;
    }

    public static async UniTask CanvasFadeIn(CanvasGroup _CanvasGroup, float _Duration)
    {
        try
        {
            _CanvasGroup.alpha = 0f;
            _CanvasGroup.interactable = false;

            await UniTask.WhenAll(
                _CanvasGroup.DOFade(1f, _Duration)
                .SetEase(Ease.OutQuint)
                //.OnComplete(() => _CanvasGroup.interactable = false)
                .ToUniTask()
            );
        }
        catch (Exception ex)
        {
            Debug.LogError($"### exception occurred: {ex}");
        }
    }

    public static async UniTask CanvasFadeOut(CanvasGroup _CanvasGroup, float _Duration)
    {
        try
        {
            _CanvasGroup.alpha = 1f;
            _CanvasGroup.interactable = false;

            await UniTask.WhenAll(
                _CanvasGroup.DOFade(0f, _Duration)
                .SetEase(Ease.OutQuint)
                //.OnComplete(() => { _CanvasGroup.interactable = false;
                //    if (_OnComplete != null)
                //    {
                //        Action.Add(async () =>
                //        {
                //            await _OnComplete();
                //        });
                //    }
                //})
                .ToUniTask()
            );
        }
        catch (Exception ex)
        {
            Debug.LogError($"### exception occurred: {ex}");
        }
    }
}
