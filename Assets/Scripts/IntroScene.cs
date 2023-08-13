using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class IntroScene : MonoBehaviour
{
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

            SceneController.Instance.AddLoadingTask(UniTask.Defer(async () =>
            {
                Debug.Log("1차 태스크");
                await UniTask.Delay(TimeSpan.FromMilliseconds(3000));
                Debug.Log("1차 태스크 끝");
            }));

            SceneController.Instance.AddLoadingTask(UniTask.Defer(async () =>
            {
                Debug.Log("2차 태스크");
                await UniTask.Delay(TimeSpan.FromMilliseconds(1000));
                Debug.Log("2차 태스크 끝");
            }));

            SceneController.Instance.AddLoadingTask(UniTask.Defer(async () =>
            {
                Debug.Log("3차 태스크");
                await UniTask.Delay(TimeSpan.FromMilliseconds(10000));
                Debug.Log("3차 태스크 끝");
            }));

            SceneController.Instance.LoadScene(Define.Scene.Lobby, true).Forget();
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
