using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class IntroManager : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup canvasGroup = null;

    [SerializeField]
    private float fadeDuration = 0f;

    private CancellationTokenSource cancellationToken;

    private async void Start()
    {
        CommonManager.Instance.Initialize();

        this.cancellationToken = new CancellationTokenSource();


        await SceneController.CanvasFadeIn(this.canvasGroup, fadeDuration, cancellationToken);

        await UniTask.Delay(TimeSpan.FromSeconds(3f));

        await SceneController.CanvasFadeOut(this.canvasGroup, fadeDuration, cancellationToken);

        // TODO : 유저 데이터 로드 ?
        //SceneController.LoadingTask.Add();
        
        SceneController.LoadSceneWithLoading(Define.Scene.Lobby).Forget();
    }


    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            this.cancellationToken.Cancel();
        }
    }
}
