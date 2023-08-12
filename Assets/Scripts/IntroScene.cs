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



    private void Awake()
    {
        // 싱글톤 객체 생성 및 초기화
        CommonManager.Instance.Initialize();
        SceneController.Instance.Initialize();
    }

    private void Start()
    {
        this.cancellationToken = new CancellationTokenSource();

        InitializeAsync().Forget();
    }

    private async UniTask InitializeAsync()
    {
        try
        {
            await SceneController.Instance.Fade(true, fadeDuration, true, cancellationToken);

            await UniTask.Delay(TimeSpan.FromSeconds(3f));

            await SceneController.Instance.Fade(false, fadeDuration, true, cancellationToken);

            // TODO : 유저 데이터 로드 ?
            //SceneController.LoadingTask.Add();

            // 빈 UniTask 을 넘겨줘서 바로 실행되게
            await SceneController.Instance.SceneActivation(UniTask.CompletedTask);

            SceneController.Instance.LoadScene(Define.Scene.Lobby, true).Forget();
            //SceneController.Instance.LoadSceneWithLoading(Define.Scene.Lobby).Forget();
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
