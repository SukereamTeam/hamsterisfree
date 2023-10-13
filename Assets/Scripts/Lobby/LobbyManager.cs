using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Threading;
using TMPro;

public class LobbyManager : MonoSingleton<LobbyManager>
{
    [SerializeField]
    private float fadeDuration = 0f;

    public float FadeDuration => this.fadeDuration;

    [SerializeField]
    private TextMeshProUGUI rewardText = null;

    [SerializeField]
    private InitScrollLobby initScroll = null;


    public const float BGM_VOLUME = 0.3f;

    private CancellationTokenSource cancellationToken;
    public CancellationTokenSource Cts => this.cancellationToken;


    private void Start()
    {
        this.cancellationToken = new CancellationTokenSource();
        
        Initialize();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        this.cancellationToken.Cancel();
        this.cancellationToken.Dispose();
    }

    private void Initialize()
    {
        Debug.Log("# Lobby Initialize #");

        SoundManager.Instance.Play(Define.SoundPath.BGM_LOBBY.ToString(), _FadeTime: this.fadeDuration, _Loop: true, _Volume: BGM_VOLUME).Forget();

        this.rewardText.text = $"Reward : {UserDataManager.Instance.CurUserData.rewardCount.ToString()}";

        this.initScroll.Initialize(DataContainer.Instance.StageTable.list.Count);
    }


}
