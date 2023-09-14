using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Threading;

public class LobbyManager : MonoBehaviour
{
    [SerializeField]
    private float fadeDuration = 0f;


    private CancellationTokenSource cancellationToken;



    private void Start()
    {
        this.cancellationToken = new CancellationTokenSource();
    }

    public async void OnClick_Next(int _Index)
    {
        if (CommonManager.Instance.CurUserData.curStage < _Index)
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
