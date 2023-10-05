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


    public AudioClip UiSound { get; private set; }

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

        var bgm = DataContainer.Instance.SoundTable.FindAudioClipWithName(LOBBY_BGM);
        if (bgm != null)
        {
            SoundManager.Instance.Play(bgm, (int)Define.SoundIndex.Common_Bgm, _Loop: true, _IsVolumeFade: true, _Volume: 0.3f).Forget();
        }
        else
        {
            Debug.Log($"### Not Found {LOBBY_BGM} ###");
        }

        UiSound = DataContainer.Instance.SoundTable.FindAudioClipWithName(LOBBY_SFX);
        if (UiSound == null)
        {
            Debug.LogError($"### Not Found {LOBBY_SFX} ###");
        }

        this.rewardText.text = $"Reward : {UserDataManager.Instance.CurUserData.rewardCount.ToString()}";
        
        this.initScroll.Initialize(DataContainer.Instance.StageTable.list.Count);
    }
}
