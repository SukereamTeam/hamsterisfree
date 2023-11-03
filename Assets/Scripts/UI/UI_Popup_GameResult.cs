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

    private const float TWEEN_DURATION = 0.05f;
    private const float TWEEN_SCALE = 1.2f;
    private const float SEED_SCALE_DURATION = 20000;
    private const string NUMBER_FORMAT = "D3";



    private void OnDestroy()
    {
        DOTween.KillAll();

        this.cts?.Cancel();
        this.cts?.Dispose();
    }

    public void Initialize(int _StageNumber, int _SeedCount, int _Score)
    {
        Reset();

        this.cts ??= new CancellationTokenSource();

        this.stageNumberText.text = $"STAGE {_StageNumber.ToString(NUMBER_FORMAT)}";

        this.scoreText.text = _Score.ToString();

        if (this.gameObject.activeSelf == false)
            this.gameObject.SetActive(true);

        this.transform.DOScale(TWEEN_SCALE, TWEEN_DURATION).SetEase(Ease.OutBounce).OnComplete(() =>
        {
            this.transform.DOScale(1f, TWEEN_DURATION).SetEase(Ease.InBounce);

            // 먹은 씨앗 갯수 만큼 이미지 채워지는 연출 Flow
            SeedFlowAsync(_SeedCount).Forget();
        });
    }

    public void Hide()
    {
        this.transform.DOScale(0f, TWEEN_DURATION).SetEase(Ease.InBounce);
    }

    public async void OnClick_Lobby()
    {
        SoundManager.Instance.PlayOneShot(Define.SoundPath.SFX_BACK_BUTTON.ToString()).Forget();

        await this.lobbyButton.transform.DOScale(TWEEN_SCALE, TWEEN_DURATION).OnComplete(async () =>
        {
            await this.lobbyButton.transform.DOScale(1f, TWEEN_DURATION);

            SoundManager.Instance.Stop(GameManager.Instance.BgmPath);

            await SceneController.Instance.Fade(false, TWEEN_DURATION, false);

            SceneController.Instance.LoadScene(Define.Scene.Lobby, false).Forget();
        });
    }

    public async void OnClick_Next()
    {
        await this.nextButton.transform.DOScale(TWEEN_SCALE, TWEEN_DURATION).OnComplete(async () =>
        {
            await this.nextButton.transform.DOScale(1f, TWEEN_DURATION);

            SoundManager.Instance.Stop(GameManager.Instance.BgmPath);
        });
    }

    private async UniTaskVoid SeedFlowAsync(int _SeedCount)
    {
        try
        {
            // TODO : 흑백 씨앗이 색깔 채워지며 커졌다 돌아오는 연출
            for (int i = 0; i < _SeedCount; i++)
            {
                if (this.cts.IsCancellationRequested == true)
                    return;

                await this.seedArray[i].DOScale(TWEEN_SCALE, TWEEN_DURATION)
                    .SetEase(Ease.InOutBounce)
                    .OnComplete(async () =>
                    {
                        if (this.cts?.IsCancellationRequested == true)
                            return;

                        await UniTask.Delay(TimeSpan.FromMilliseconds(SEED_SCALE_DURATION), cancellationToken: this.cts.Token);
                    });

                await UniTask.Yield();
            }
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            Debug.LogError($"### exception occurred: {ex.Message} / {ex.StackTrace}");
        }
    }

    private void Reset()
    {
        this.transform.localScale = Vector3.zero;

        if (this.gameObject.activeSelf == true)
            this.gameObject.SetActive(false);

        this.cts?.Cancel();
        this.cts?.Dispose();
        this.cts = null;
    }
}
