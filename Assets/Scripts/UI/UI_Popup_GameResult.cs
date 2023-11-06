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
    private Tween[] scaleTweens = new Tween[3];
    private Tween popupTween = null;
    private int stageNumber = -1;

    private const float TWEEN_DURATION = 5f;
    private const float TWEEN_SCALE = 1.2f;
    private const float SEED_SCALE_DURATION = 2f;
    private const float SCENE_CHANGE_DURATION = 0.5f;

    private const string NUMBER_FORMAT = "D3";



    private void OnDestroy()
    {
        this.cts?.Cancel();
        this.cts?.Dispose();

        for (int i = 0; i < this.scaleTweens.Length; i++)
        {
            this.scaleTweens[i]?.Kill();
        }

        this.popupTween?.Kill();
    }

    private void OnDisable()
    {
        for (int i = 0; i < this.scaleTweens.Length; i++)
        {
            this.scaleTweens[i]?.Kill();
            this.scaleTweens[i] = null;

            this.seedArray[i].localScale = Vector3.one;
        }

        this.popupTween?.Kill();
        this.popupTween = null;
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

        Sequence sequence = DOTween.Sequence()
            .Append(this.popupRoot.DOScale(TWEEN_SCALE, TWEEN_DURATION).SetEase(Ease.OutBounce))
            .Append(this.popupRoot.DOScale(1f, TWEEN_DURATION).SetEase(Ease.InBounce));

        this.popupTween = sequence;

        Debug.Log("Popup Tween Play Start");
        await this.popupTween.Play().ToUniTask(cancellationToken: this.cts.Token);

        Debug.Log("Popup Tween Play Stop");
        SeedFlowAsync(_StarCount).Forget();

        //this.popupTween = this.popupRoot.DOScale(TWEEN_SCALE, TWEEN_DURATION).SetEase(Ease.OutBounce).OnComplete(() =>
        //{
        //    this.popupRoot.DOScale(1f, TWEEN_DURATION).SetEase(Ease.InBounce);

        //    // 이 스테이지에서 총 얻게 된 별 갯수 만큼 이미지 채워지는 연출 Flow
        //    SeedFlowAsync(_StarCount).Forget();
        //});
    }

    public void Hide()
    {
        this.popupTween = this.popupRoot.DOScale(0f, TWEEN_DURATION).SetEase(Ease.InBounce).OnComplete(() =>
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
        await UniTask.Delay(TimeSpan.FromMilliseconds(500), cancellationToken: this.cts.Token);


        // TODO : 흑백 씨앗이 색깔 채워지며 커졌다 돌아오는 연출
        for (int i = 0; i < _SeedCount; i++)
        {
            if (this.cts == null || this.cts?.Token.IsCancellationRequested == true)
            {
                return;
            }

            Debug.Log($"### Seed {i} 1.2 Scale Start ###");

            Sequence seedSequence = DOTween.Sequence()
            .Append(this.seedArray[i].DOScale(TWEEN_SCALE, SEED_SCALE_DURATION))
            .Append(this.seedArray[i].DOScale(1f, SEED_SCALE_DURATION));

            this.scaleTweens[i] = seedSequence;

            await this.scaleTweens[i].Play().ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());

            //this.scaleTweens[i] = this.seedArray[i].DOScale(TWEEN_SCALE, SEED_SCALE_DURATION);
            //.SetEase(Ease.InOutBounce).OnComplete(() =>
            //{
            //    if (this.cts == null || this.cts?.Token.IsCancellationRequested == false)
            //    {
            //        this.seedArray[i].DOScale(1f, SEED_SCALE_DURATION);
            //        Debug.Log($"### Seed {i} one Scale Done ###");
            //    }
            //});


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
