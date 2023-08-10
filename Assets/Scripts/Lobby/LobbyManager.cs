using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup canvasGroup = null;

    [SerializeField]
    private float fadeDuration = 0f;

    public async void OnClick_Next()
    {
        await SceneController.CanvasFadeOut(this.canvasGroup, this.fadeDuration);

        SceneController.LoadingTask.Add(UniTask.Defer(DataContainer.LoadStageDatas));
        SceneController.LoadScene(Define.Scene.Game).Forget();//, async () =>
        //{
        //    SceneController.Operation.allowSceneActivation = true;

        //    await UniTask.CompletedTask;
        //}).Forget();
    }
}
