using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class IntroScene : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup canvasGroup = null;

    [SerializeField]
    private float fadeDuration = 0f;

    private CancellationTokenSource cancellationToken;

    private void Start()
    {
        CommonManager.Instance.Initialize();

        this.cancellationToken = new CancellationTokenSource();

        InitializeAsync().Forget();
    }

    private async UniTask InitializeAsync()
    {
        try
        {
            await SceneController.Instance.CanvasFadeIn(this.canvasGroup, fadeDuration, cancellationToken, () =>
            {
                this.canvasGroup.alpha = 1f;
            });

            await UniTask.Delay(TimeSpan.FromSeconds(3f));

            await SceneController.Instance.CanvasFadeOut(this.canvasGroup, fadeDuration, cancellationToken, () =>
            {
                this.canvasGroup.alpha = 1f;
            });

            // TODO : 유저 데이터 로드 ?
            //SceneController.LoadingTask.Add();

            // 빈 UniTask 을 넘겨줘서 바로 실행되게
            await SceneController.Instance.SceneActivation(UniTask.CompletedTask);

            SceneController.Instance.LoadSceneWithLoading(Define.Scene.Lobby).Forget();
        }
        catch (Exception ex)
        {
            Debug.LogError($"### exception occurred: {ex}");
        }
    }


    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            this.cancellationToken.Cancel();
        }
    }
}
