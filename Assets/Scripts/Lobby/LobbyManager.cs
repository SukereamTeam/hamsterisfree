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
    private CancellationTokenSource cancellationToken;



    private void Start()
    {
        this.cancellationToken = new CancellationTokenSource();

        this.rewardText.text = $"Reward : {UserDataManager.Instance.CurUserData.rewardCount.ToString()}";

        var bgm = DataContainer.Instance.SoundTable.FindAudioClipWithName(LOBBY_BGM);

        if (bgm != null)
        {
            SoundManager.Instance.Play(bgm, true, true).Forget();
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

        CommonManager.Instance.CurStageIndex = _Index;
        
        await SceneController.Instance.Fade(false, this.fadeDuration, false, cancellationToken);

        SceneController.Instance.AddLoadingTask(UniTask.Defer(() => DataContainer.Instance.LoadStageDatas(_Index)));

        SceneController.Instance.LoadScene(Define.Scene.Game, false).Forget();
    }
}
