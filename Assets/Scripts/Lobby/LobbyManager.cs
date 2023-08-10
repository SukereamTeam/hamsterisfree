using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

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

        await UniTask.Yield();

        SceneController.LoadingTask.Add(UniTask.Defer(DataContainer.LoadStageDatas));
        SceneController.LoadScene(Define.Scene.Game).Forget();//, async () =>
        //{
        //    SceneController.Operation.allowSceneActivation = true;

        //    await UniTask.CompletedTask;
        //}).Forget();
    }
}
