using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using UniRx;

public class LobbyManager : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup canvasGroup = null;

    [SerializeField]
    private float fadeDuration = 0f;


    private CancellationTokenSource cancellationToken;



    private void Start()
    {
        this.cancellationToken = new CancellationTokenSource();
    }

    public async void OnClick_Next()
    {
        await SceneController.CanvasFadeOut(this.canvasGroup, this.fadeDuration, cancellationToken);

        var task = DataContainer.LoadStageDatas();
        await SceneController.SceneActivation(task);

        SceneController.LoadingTask.Add(UniTask.Defer(() => UniTask.FromResult(task)));

        SceneController.LoadScene(Define.Scene.Game).Forget();
    }
}
