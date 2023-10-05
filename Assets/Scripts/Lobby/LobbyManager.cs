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



    public const string LOBBY_BGM = "Lobby_BGM";
    public const string LOBBY_SFX = "STAGE_ENTER_SFX";
    public const int ENTER_SFX_IDX = 2;

    private CancellationTokenSource cancellationToken;
    public CancellationTokenSource Cts => this.cancellationToken;


    private void Start()
    {
        this.cancellationToken = new CancellationTokenSource();
        
        Initialize();
    }

    private void Initialize()
    {
        Debug.Log("# Lobby Initialize #");

        SoundManager.Instance.Play(LOBBY_BGM, _Loop: true, _Volume: 0.3f).Forget();

        this.rewardText.text = $"Reward : {UserDataManager.Instance.CurUserData.rewardCount.ToString()}";

        this.initScroll.Initialize(DataContainer.Instance.StageTable.list.Count);
    }
}
