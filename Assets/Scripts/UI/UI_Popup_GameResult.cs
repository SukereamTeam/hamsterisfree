using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

public class UI_Popup_GameResult : MonoBehaviour
{
    [SerializeField]
    private Transform popupRoot = null;

    [SerializeField]
    private Button lobbyButton = null;

    [SerializeField]
    private Button nextButton = null;

    [SerializeField]
    private TextMeshProUGUI nextButtonText = null;

    [SerializeField]
    private TextMeshProUGUI stageNumberText = null;

    [SerializeField]
    private TextMeshProUGUI scoreText = null;

    [SerializeField]
    private Transform[] seedArray = null;


    private CancellationTokenSource cts = null;
    private int stageNumber = -1;
    private bool canNext = false;

    private const float TWEEN_DURATION = 0.1f;
    private const float TWEEN_SCALE = 1.2f;
    private const float SEED_SCALE_DURATION = 0.2f;
    private const float SCENE_CHANGE_DURATION = 0.5f;

    private const string NUMBER_FORMAT = "D3";      // 스테이지 번호를 001, 002 형식으로 출력하고 싶어서



    private void OnDestroy()
    {
        this.cts?.Cancel();
        this.cts?.Dispose();
    }

    public async UniTaskVoid Initialize(int _StageNumber, int _StarCount, int _Score)
    {
        Reset();

        this.cts ??= new CancellationTokenSource();

        SettingPopupValue(_StageNumber, _Score);

        if (this.gameObject.activeSelf == false)
            this.gameObject.SetActive(true);


        await this.popupRoot.DOScale(1f, TWEEN_DURATION * 3f).SetEase(Ease.InCubic).WithCancellation(this.cts.Token);


        SeedFlowAsync(_StarCount).Forget();
    }

    public void Hide()
    {
        this.popupRoot.DOScale(0f, TWEEN_DURATION * 3f).SetEase(Ease.InCubic).OnComplete(() =>
        {
            if (this.cts == null || this.cts?.IsCancellationRequested == false)
            {
                Reset();
            }
        }).WithCancellation(this.cts.Token);
    }

    public async void OnClick_LobbyAsync()
    {
        SoundManager.Instance.Stop(GameManager.Instance.BgmPath);
        SoundManager.Instance.PlayOneShot(Define.SoundPath.SFX_BACK_BUTTON.ToString()).Forget();

        await this.lobbyButton.transform.DOScale(TWEEN_SCALE, TWEEN_DURATION).OnComplete(async () =>
        {
            await this.lobbyButton.transform.DOScale(1f, TWEEN_DURATION);

            Hide();

            await SceneController.Instance.Fade(false, SCENE_CHANGE_DURATION, false);

            SceneController.Instance.LoadScene(Define.Scene.Lobby, false).Forget();

            
        });
    }

    public async void OnClick_NextAsync()
    {
        SoundManager.Instance.Stop(GameManager.Instance.BgmPath);
        SoundManager.Instance.PlayOneShot(Define.SoundPath.SFX_ENTER_STAGE.ToString()).Forget();


        var nextIndex = this.canNext == true ? this.stageNumber + 1 : this.stageNumber;


        CommonManager.Instance.CurStageIndex = nextIndex;

        await this.nextButton.transform.DOScale(TWEEN_SCALE, TWEEN_DURATION).OnComplete(async () =>
        {
            await this.nextButton.transform.DOScale(1f, TWEEN_DURATION);

            Hide();

            await SceneController.Instance.Fade(false, SCENE_CHANGE_DURATION, false);



            SceneController.Instance.AddLoadingTask(UniTask.Defer(() => DataContainer.Instance.LoadStageDatas(nextIndex)));

            SceneController.Instance.LoadScene(Define.Scene.Game, false).Forget();
        });
    }

    private void SettingPopupValue(int _StageNumber, int _Score)
    {
        // set UI popup value
        this.stageNumber = _StageNumber;
        this.stageNumberText.text = $"STAGE {(_StageNumber + 1).ToString(NUMBER_FORMAT)}";

        this.scoreText.text = _Score.ToString();

        // 게임 스코어에 따른 사운드 재생
        if (_Score > 0)
            SoundManager.Instance.PlayOneShot(Define.SoundPath.SFX_GAME_END.ToString()).Forget();
        else
            SoundManager.Instance.PlayOneShot(Define.SoundPath.SFX_GAME_END_FAIL.ToString()).Forget();


        // 다음 스테이지로 이동할 수 있는지 체크하여 nextButton에 next 혹은 retry 기능 넣어주기
        this.canNext = CheckCanNextStage(_StageNumber, _Score);
        if (this.canNext == true)
            this.nextButtonText.text = $"다음 스테이지";
        else
            this.nextButtonText.text = $"재시작";
    }

    private async UniTaskVoid SeedFlowAsync(int _SeedCount)
    {
        await UniTask.Delay(TimeSpan.FromMilliseconds(500), cancellationToken: this.cts.Token);

        // TODO : 흑백 씨앗이 색깔 채워지며 커졌다 돌아오는 연출
        for (int i = 0; i < _SeedCount; i++)
        {
            if (this.cts == null || this.cts?.IsCancellationRequested == true)
            {
                return;
            }

            await this.seedArray[i].DOScale(TWEEN_SCALE, SEED_SCALE_DURATION)
            .SetEase(Ease.OutCirc).OnComplete(() =>
            {
                if (this.cts == null || this.cts?.IsCancellationRequested == false)
                {
                    this.seedArray[i].DOScale(1f, SEED_SCALE_DURATION);
                }
            });


            await UniTask.Yield();
        }
    }

    private bool CheckCanNextStage(int _StageNumber, int _Score)
    {
        var curUserData = UserDataManager.Instance.CurUserData;

        if (_Score > 0)
        {
            return true;
        }
        else
        {
            if (_StageNumber < curUserData.curStage)
            {
                // 현재 클리어해야 할 스테이지보다 낮은 스테이지를 클리어 한 것
                // 과거 클리어 했던 스테이지를 또 클리어 한거니까 다음 스테이지로 이동 할 수 있음

                return true;
            }
            else
            {
                return false;
            }
        }
    }

    private void Reset()
    {
        Debug.Log("### UI Reset ###");

        this.popupRoot.localScale = Vector3.zero;

        if (this.gameObject.activeSelf == true)
            this.gameObject.SetActive(false);

        this.cts?.Cancel();
        this.cts = null;
    }
}
