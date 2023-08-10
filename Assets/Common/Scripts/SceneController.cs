using System.Collections;
using System.Collections.Generic;
using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class SceneController : MonoBehaviour
{

    [SerializeField]
    private TextMeshProUGUI currentText = null;

    [SerializeField]
    private Slider progressBar = null;

    [SerializeField]
    private CanvasGroup canvasGroup = null;

    [SerializeField]
    private float fadeDuration = 0.5f;


    private static string nextScene;

    public static List<UniTask> LoadingTask = new List<UniTask>();
    private static AsyncOperation operation;




    private async void Start()
    {
        LoadingTask.Add(UniTask.Defer(LoadSceneAsync));

        await UniTask.WhenAll(LoadingTask.ToArray());

        LoadingTask.Clear();
    }

    public static async UniTaskVoid LoadScene(Define.Scene _SceneName)
    {
        var sceneString = Enum.GetName(typeof(Define.Scene), _SceneName);

        nextScene = sceneString;

        LoadingTask.Add(UniTask.Defer(LoadSceneAsync));

        await UniTask.WhenAll(LoadingTask.ToArray());

        LoadingTask.Clear();
    }

    public static void LoadSceneWithLoading(Define.Scene _SceneName)
    {
        var sceneString = Enum.GetName(typeof(Define.Scene), _SceneName);

        nextScene = sceneString;

        SceneManager.LoadScene("Loading");
    }


    private static async UniTask LoadSceneAsync()
    {
        operation = SceneManager.LoadSceneAsync(nextScene);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f); // allowSceneActivation이 false일 때까지 진행률을 0.9까지 제한합니다.

            if (progress >= 0.9f)
            {
                Debug.Log("다 됐는디요 행님");

                break;
            }

            await UniTask.Yield(); // 다음 프레임까지 대기
        }

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
                //.OnComplete(() => _CanvasGroup.interactable = false)
                .ToUniTask()
            );
        }
        catch (Exception ex)
        {
            Debug.LogError($"### exception occurred: {ex}");
        }
    }
}
