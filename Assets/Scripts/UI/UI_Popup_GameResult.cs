using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using System.Threading;
using System;

public class UI_Popup_GameResult : MonoBehaviour
{
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

    private const float TWEEN_DURATION = 0.05f;
    private const float TWEEN_SCALE = 1.2f;
    private const float SEED_SCALE_DURATION = 20000;
    private const float SCENE_CHANGE_DURATION = 0.5f;

    private const string NUMBER_FORMAT = "D3";



    private void OnDestroy()
    {
        this.cts?.Cancel();
        this.cts?.Dispose();
    }

    public void Initialize(int _StageNumber, int _StarCount, int _Score)
    {
        Reset();

        this.cts ??= new CancellationTokenSource();

        this.stageNumber = _StageNumber;
        this.stageNumberText.text = $"STAGE {_StageNumber.ToString(NUMBER_FORMAT)}";

        this.scoreText.text = _Score.ToString();

        if (this.gameObject.activeSelf == false)
            this.gameObject.SetActive(true);

        this.transform.DOScale(TWEEN_SCALE, TWEEN_DURATION).SetEase(Ease.OutBounce).OnComplete(() =>
        {
            this.transform.DOScale(1f, TWEEN_DURATION).SetEase(Ease.InBounce);

            // 이 스테이지에서 총 얻게 된 별 갯수 만큼 이미지 채워지는 연출 Flow
            SeedFlowAsync(_StarCount).Forget();
        });
    }

    public void Hide()
    {
        this.transform.DOScale(0f, TWEEN_DURATION).SetEase(Ease.InBounce).OnComplete(() =>
        {
            Reset();
        });
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
        // TODO : 흑백 씨앗이 색깔 채워지며 커졌다 돌아오는 연출
        for (int i = 0; i < _SeedCount; i++)
        {
            await this.seedArray[i].DOScale(TWEEN_SCALE, TWEEN_DURATION)
                .SetEase(Ease.InOutBounce)
                .OnComplete(async () =>
                {
                    await UniTask.Delay(TimeSpan.FromMilliseconds(SEED_SCALE_DURATION), cancellationToken: this.cts.Token);
                });

            await UniTask.Yield();
        }
    }

    private void Reset()
    {
        Debug.Log("### UI Reset ###");

        this.transform.localScale = Vector3.zero;

        if (this.gameObject.activeSelf == true)
            this.gameObject.SetActive(false);

        this.cts?.Cancel();
        this.cts?.Dispose();
        this.cts = null;
    }
}
