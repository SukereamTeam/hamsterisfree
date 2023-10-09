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

    //private float[] itemWidths = new float[3] { 150f, 200f, 150f };
    private const float STAGE_ITEM_SIZE = 150f;
    private const float STAGE_ITEM_SIZE_CUR = 200f;
    
    private int stageIndex = -1;

    private Image itemImage = null;



    private void Awake()
    {
        this.itemImage = this.GetComponent<Image>();
        if (this.itemImage == null)
        {
            Debug.Log("### Not Found StageItem Image Component. ###");
        }
    }

    private void ScrollCellIndex(int _Index)
    {
        if (this.lobbyManager == null)
        {
            this.lobbyManager = LobbyManager.Instance;
        }
        
        this.layoutElement.preferredWidth = STAGE_ITEM_SIZE;//itemWidths[Mathf.Abs(_Index) % 3];
        this.layoutElement.preferredHeight = STAGE_ITEM_SIZE;//itemWidths[Mathf.Abs(_Index) % 3];
        
        this.stageIndex = _Index;

        this.stageItemText.text = $"Stage {_Index}";

        if (UserDataManager.Instance.CurUserData.curStage < this.stageIndex)
        {
            this.itemImage.color = Color.yellow;
        }
        else if (UserDataManager.Instance.CurUserData.curStage == this.stageIndex)
        {
            this.itemImage.color = Color.white;
            
            this.layoutElement.preferredWidth = STAGE_ITEM_SIZE_CUR;
            this.layoutElement.preferredHeight = STAGE_ITEM_SIZE_CUR;
        }
        else
        {
            this.itemImage.color = Color.blue;
        }
    }

    public async void OnClick_ItemAsync()
    {
        if (UserDataManager.Instance.CurUserData.curStage < this.stageIndex)
        {
            Debug.Log("아직 이전 스테이지 클리어 안 함!");
            
            return;
        }

        PlaySoundEnterStage();

        CommonManager.Instance.CurStageIndex = this.stageIndex;
        
        await SceneController.Instance.Fade(false, this.lobbyManager.FadeDuration, false, this.lobbyManager.Cts);

        SceneController.Instance.AddLoadingTask(UniTask.Defer(() => DataContainer.Instance.LoadStageDatas(this.stageIndex)));

        SceneController.Instance.LoadScene(Define.Scene.Game, false).Forget();
    }

    private void PlaySoundEnterStage()
    {
        SoundManager.Instance.PlayOneShot(LobbyManager.LOBBY_SFX).Forget();

        SoundManager.Instance.Stop(LobbyManager.LOBBY_BGM,
            this.lobbyManager.FadeDuration);
    }
}
