using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class IntroManager : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup canvasGroup = null;

    [SerializeField]
    private float fadeDuration = 0f;

    private async void Start()
    {
        CommonManager.Instance.Initialize();

        await SceneController.CanvasFadeIn(this.canvasGroup, fadeDuration);

        await UniTask.Delay(3000);

        await SceneController.CanvasFadeOut(this.canvasGroup, fadeDuration);

        // TODO : 유저 데이터 로드 ?
        //SceneController.LoadingTask.Add();
        SceneController.LoadSceneWithLoading(Define.Scene.Lobby);
    }
}
