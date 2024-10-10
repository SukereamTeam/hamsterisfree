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

    private CancellationTokenSource fadeCts;

    private const float LOGO_DELAY_TIME = 1.5f;



    

    private void Start()
    {
        this.fadeCts = new CancellationTokenSource();

        SDKFirebase.Instance.Initialize();
        InitializeAsync().Forget();
    }

    private async UniTask InitializeAsync()
    {
        try
        {
            await SceneController.Instance.Fade(true, this.fadeDuration, true, this.fadeCts);

            Sequence shakeSequence = DOTween.Sequence();

            if (this.logo != null)
            {
                _ = shakeSequence.Append(this.logo?.transform.DOScale(Vector3.one * 1.2f, 0.1f));
                _ = shakeSequence.Append(this.logo?.transform.DOScale(Vector3.one, 0.1f));
                _ = shakeSequence.ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());
                _ = shakeSequence.Play().SetLoops(2, LoopType.Restart);
            }
            
            await UniTask.Delay(TimeSpan.FromSeconds(LOGO_DELAY_TIME), cancellationToken: this.GetCancellationTokenOnDestroy());

            await SceneController.Instance.Fade(false, fadeDuration, true, this.fadeCts);

            SceneController.Instance.AddLoadingTask(UniTask.Defer(async () =>
            {
                // CommonManager 싱글톤 객체 생성 및 초기화
                CommonManager.Instance.Initialize();
                SoundManager.Instance.Initialize();
                
                await UniTask.Yield();
            }));

            SceneController.Instance.LoadScene(Define.Scene.Lobby, true).Forget();
        }
        catch (Exception ex) when(!(ex is OperationCanceledException))      // 실행되는 도중 꺼버릴 경우 UniTask.Delay가 exception throw 해서 무시하도록 처리
        {
            Debug.Log("### Intro Scene Exception : {" + ex.Message + ex.StackTrace + "} ###");
        }
    }


    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && this.fadeCts?.IsCancellationRequested == false)
        {
            this.fadeCts.Cancel();
        }
    }

    private void OnDestroy()
    {
        DOTween.KillAll(true);
        this.fadeCts?.Cancel();
        this.fadeCts?.Dispose();
    }
}
