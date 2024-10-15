using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TMPro;
using UniRx;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LobbyManager : MonoSingleton<LobbyManager>
{
    [SerializeField]
    private float fadeDuration = 0f;

    public float FadeDuration => this.fadeDuration;

    [SerializeField]
    private TextMeshProUGUI rewardText = null;

    [SerializeField]
    private InitScrollLobby initScroll = null;

    [SerializeField] private Button infoButton;


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

        infoButton.OnClickAsObservable().Subscribe(_ =>
        {
            // TODO : info 팝업 생성 및 로그아웃 기능 버튼 추가 필요
            
            var auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
            auth?.SignOut();
            
            JsonManager.Instance.RemoveData<UserData>();
            JsonManager.Instance.RemoveData<StageData>();
            
            SceneController.Instance.LoadScene(Define.Scene.Intro, false).Forget();
        }).AddTo(this);
    }


}
