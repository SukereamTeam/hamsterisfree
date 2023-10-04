using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Threading;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    [SerializeField]
    private float fadeDuration = 0f;

    [SerializeField]
    private TextMeshProUGUI rewardText = null;


    

    private const string LOBBY_BGM = "Lobby_BGM";
    private const string LOBBY_SFX = "Stage_Enter_SFX";
    private const int ENTER_SFX_IDX = 2;

    private CancellationTokenSource cancellationToken;



    private void Start()
    {
        this.cancellationToken = new CancellationTokenSource();

        this.rewardText.text = $"Reward : {UserDataManager.Instance.CurUserData.rewardCount.ToString()}";

        var bgm = DataContainer.Instance.SoundTable.FindAudioClipWithName(LOBBY_BGM);

        if (bgm != null)
        {
            SoundManager.Instance.Play(bgm, (int)Define.SoundIndex.Common_Bgm, _Loop: true, _IsVolumeFade: true, _Volume: 0.3f).Forget();
        }
        else
        {
            Debug.Log($"### Not Found {LOBBY_BGM} ###");
        }
        
    }

    public async void OnClick_Next(int _Index)
    {
        if (UserDataManager.Instance.CurUserData.curStage < _Index)
        {
            Debug.Log("아직 이전 스테이지 클리어 안 함!");

            return;
        }

        var sfx = DataContainer.Instance.SoundTable.FindAudioClipWithName(LOBBY_SFX);

        if (sfx != null)
        {
            SoundManager.Instance.PlayOneShot(sfx, ENTER_SFX_IDX).Forget();
        }
        else
        {
            Debug.Log($"### Not Found {LOBBY_SFX} ###");
        }

        SoundManager.Instance.FadeVolumeStart(false,
                SoundManager.Instance.AudioSourceList[(int)Define.SoundIndex.Common_Bgm].volume,
                SoundManager.Instance.AudioSourceList[(int)Define.SoundIndex.Common_Bgm],
                this.fadeDuration, () =>
                {
                    SoundManager.Instance.Stop((int)Define.SoundIndex.Common_Bgm);
                }
            );

        CommonManager.Instance.CurStageIndex = _Index;
        
        await SceneController.Instance.Fade(false, this.fadeDuration, false, this.cancellationToken);

        SceneController.Instance.AddLoadingTask(UniTask.Defer(() => DataContainer.Instance.LoadStageDatas(_Index)));

        SceneController.Instance.LoadScene(Define.Scene.Game, false).Forget();
    }
}
