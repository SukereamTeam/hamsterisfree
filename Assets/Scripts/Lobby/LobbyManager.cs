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
        await SceneController.Instance.Fade(false, this.fadeDuration, false, cancellationToken);

        // TODO
        // 데이터 로드 같은 작업들만 ListTask에 넣어주고 WhenAlll
        // 그다음에 씬 로드 하도록 변경

        //var task = DataContainer.LoadStageDatas();
        //await SceneController.Instance.SceneActivation(task);

        //SceneController.Instance.AddLoadingTask(UniTask.Defer(() => UniTask.FromResult(task)));

        SceneController.Instance.AddLoadingTask(UniTask.Defer(() => DataContainer.LoadStageDatas()));

        SceneController.Instance.LoadScene(Define.Scene.Game, false).Forget();
    }
}
