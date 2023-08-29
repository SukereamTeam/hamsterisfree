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

    public async void OnClick_Next()
    {
        await SceneController.Instance.Fade(false, this.fadeDuration, false, cancellationToken);

        // TODO : Test용
        var curStageIndex = CommonManager.Instance.CurStageIndex;
        
        SceneController.Instance.AddLoadingTask(UniTask.Defer(() => DataContainer.Instance.LoadStageDatas(curStageIndex)));

        SceneController.Instance.LoadScene(Define.Scene.Game, false).Forget();
    }
}
