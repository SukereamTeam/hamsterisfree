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
    private TextMeshProUGUI stageNumberText = null;

    [SerializeField]
    private TextMeshProUGUI scoreText = null;

    [SerializeField]
    private Transform[] seedArray = null;


    private CancellationTokenSource cts = null;
    private int stageNumber = -1;

    private const float TWEEN_DURATION = 0.3f;
    private const float TWEEN_SCALE = 1.2f;
    private const float SEED_SCALE_DURATION = 0.2f;
    private const float SCENE_CHANGE_DURATION = 0.5f;

    private const string NUMBER_FORMAT = "D3";



    private void OnDestroy()
    {
        this.cts?.Cancel();
        this.cts?.Dispose();
    }

    public async UniTaskVoid Initialize(int _StageNumber, int _StarCount, int _Score)
    {
        Reset();

        this.cts ??= new CancellationTokenSource();

        this.stageNumber = _StageNumber;
        this.stageNumberText.text = $"STAGE {_StageNumber.ToString(NUMBER_FORMAT)}";

        this.scoreText.text = _Score.ToString();

        if (this.gameObject.activeSelf == false)
            this.gameObject.SetActive(true);

        await this.popupRoot.DOScale(1f, TWEEN_DURATION).SetEase(Ease.InCubic).WithCancellation(this.cts.Token);

        SeedFlowAsync(_StarCount).Forget();
    }

    public void Hide()
    {
        this.popupRoot.DOScale(0f, TWEEN_DURATION).SetEase(Ease.InBounce).OnComplete(() =>
        {
            if (this.cts == null || this.cts?.IsCancellationRequested == false)
            {
                Reset();
            }
        }).WithCancellation(this.cts.Token);
    }

    public async void OnClick_Lobby()
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

    public async void OnClick_Next()
    {
        SoundManager.Instance.Stop(GameManager.Instance.BgmPath);
        SoundManager.Instance.PlayOneShot(Define.SoundPath.SFX_ENTER_STAGE.ToString()).Forget();

        var nextIndex = this.stageNumber + 1;
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
