using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine.UI;
using DG.Tweening;

public class IntroScene : MonoBehaviour
{
    [SerializeField]
    private float fadeDuration = 0f;

    [SerializeField] private Image logo = null;

    private CancellationTokenSource cancellationToken;



    private void Awake()
    {
        // CommonManager 싱글톤 객체 생성 및 초기화
        CommonManager.Instance.Initialize();

        SoundManager.Instance.Initialize();
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
            
            Sequence shakeSequence = DOTween.Sequence();
            shakeSequence.Append(this.logo.transform.DOScale(Vector3.one * 1.2f, 0.1f));
            shakeSequence.Append(this.logo.transform.DOScale(Vector3.one, 0.1f));
            shakeSequence.Play().SetLoops(2, LoopType.Restart);
            
            await UniTask.Delay(TimeSpan.FromSeconds(1.5f));

            await SceneController.Instance.Fade(false, fadeDuration, true, cancellationToken);

            SceneController.Instance.AddLoadingTask(UniTask.Defer(async () =>
            {
                Debug.Log("1차 태스크");
                await UniTask.Delay(TimeSpan.FromMilliseconds(2000));
                Debug.Log("1차 태스크 끝");
            }));

            SceneController.Instance.AddLoadingTask(UniTask.Defer(async () =>
            {
                Debug.Log("2차 태스크");
                await UniTask.Delay(TimeSpan.FromMilliseconds(1000));
                Debug.Log("2차 태스크 끝");
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

    private void OnDestroy()
    {
        DOTween.KillAll(true);
        this.cancellationToken.Cancel();
    }
}
