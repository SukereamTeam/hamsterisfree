using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ScrollIndexCallback_StageItem : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI stageItemText = null;

    [SerializeField]
    private LayoutElement layoutElement;

    private LobbyManager lobbyManager;
    
    private float[] itemWidths = new float[3] { 150f, 200f, 150f };
    
    private int index = -1;
    
    
    
    private void ScrollCellIndex(int _Index)
    {
        if (this.lobbyManager == null)
        {
            this.lobbyManager = LobbyManager.Instance;
        }
        
        this.layoutElement.preferredWidth = 150f;//itemWidths[Mathf.Abs(_Index) % 3];
        this.layoutElement.preferredHeight = 150f;//itemWidths[Mathf.Abs(_Index) % 3];
        
        this.index = _Index;

        this.stageItemText.text = $"Stage {_Index}";

        if (UserDataManager.Instance.CurUserData.curStage < this.index)
        {
            this.gameObject.GetComponent<Image>().color = Color.yellow;
        }
        else if (UserDataManager.Instance.CurUserData.curStage == this.index)
        {
            this.gameObject.GetComponent<Image>().color = Color.white;
            
            this.layoutElement.preferredWidth = 200f;
            this.layoutElement.preferredHeight = 200f;
        }
        else
        {
            this.gameObject.GetComponent<Image>().color = Color.blue;
        }
    }

    public async void OnClick_Item()
    {
        if (UserDataManager.Instance.CurUserData.curStage < this.index)
        {
            Debug.Log("아직 이전 스테이지 클리어 안 함!");
            
            return;
        }

        CommonManager.Instance.CurStageIndex = this.index;
        
        await SceneController.Instance.Fade(false, this.lobbyManager.FadeDuration, false, this.lobbyManager.Cts);

        SceneController.Instance.AddLoadingTask(UniTask.Defer(() => DataContainer.Instance.LoadStageDatas(this.index)));

        SceneController.Instance.LoadScene(Define.Scene.Game, false).Forget();
    }
}
