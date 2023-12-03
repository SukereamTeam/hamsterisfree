using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
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


    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        Debug.Log("# Lobby Initialize #");

        SoundManager.Instance.Play(Define.SoundPath.BGM_LOBBY.ToString(), _FadeTime: this.fadeDuration, _Loop: true, _Volume: BGM_VOLUME).Forget();

        this.rewardText.text = $"{UserDataManager.Instance.CurUserData.rewardCount.ToString()}";

        this.initScroll.Initialize(DataContainer.Instance.StageTable.list.Count);
    }


}
