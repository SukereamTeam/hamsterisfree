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
        await SceneController.CanvasFadeIn(this.canvasGroup, this.fadeDuration);

        SceneController.LoadingTask.Add(UniTask.Defer(DataContainer.LoadStageDatas));
        SceneController.LoadScene(Define.Scene.Game).Forget();
    }
}
