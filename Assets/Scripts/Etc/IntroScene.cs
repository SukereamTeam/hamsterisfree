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
                // TODO : 로그인
                var loginResult = await LoginFlow();
                if (loginResult == false)
                {
                    // 앱 종료
                }
                
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

    private async UniTask<bool> LoginFlow()
    {
        // 1. 로그인 타입이 PlayerPrefs 에 저장되어 있으면
        //     계정 정보도 저장되어있다고 판단하고 로그인을 시도한다.
        //
        //     로그인 실패 시 로그인 팝업 출력.
        //
        // 2. 로그인 타입이 저장되어 있지 않으면
        // 신규 로그인이라고 생각하고
        //     회원가입 팝업 출력.
        
        var data = PlayerPrefs.GetString("UserAccount", null);
        if (data == null)
        {
            // TODO : 첫 로그인, 로그인 UI 띄우기
            // 팝업에서 선택 결과에 따라 true, false 반환
            // 아무것도 선택 안하면 false 반환?
            var loginPopup = await CommonManager.Popup.CreateAsync<PopupLoginSelect>();
            var result = await loginPopup.ShowAsync();

            return result != false;
        }
        else
        {
            
            
            return true;
        }
    }
}
