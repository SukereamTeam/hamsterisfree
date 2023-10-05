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


    

    private const string LOBBY_BGM = "Lobby_BGM";
    private const string LOBBY_SFX = "Stage_Enter_SFX";
    private const int ENTER_SFX_IDX = 2;

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
        
        this.rewardText.text = $"Reward : {UserDataManager.Instance.CurUserData.rewardCount.ToString()}";
        
        this.initScroll.Initialize(DataContainer.Instance.StageTable.list.Count);
    }
}
