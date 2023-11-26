using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ScrollIndexCallback_StageItem : MonoBehaviour
{
    [SerializeField]
    private GameObject lockObject = null;

    [SerializeField]
    private Sprite curStageSprite = null;

    [SerializeField]
    private Sprite otherStageSprite = null;

    [SerializeField]
    private Color clearColor = Color.black;

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

        this.stageItemText.text = $"{_Index + 1}";

        this.itemImage.color = Color.white;

        if (this.lockObject.activeSelf == true)
        {
            this.lockObject.SetActive(false);
        }

        this.itemImage.color = Color.white;

        if (UserDataManager.Instance.CurUserData.curStage < this.stageIndex)
        {
            // 현재 깰 수 없는 스테이지 (남은 스테이지)
            this.lockObject.SetActive(true);

            this.itemImage.sprite = this.otherStageSprite;
            this.itemImage.color = Color.grey;
        }
        else if (UserDataManager.Instance.CurUserData.curStage == this.stageIndex)
        {
            // 현재 깨야 하는 스테이지
            this.layoutElement.preferredWidth = STAGE_ITEM_SIZE_CUR;
            this.layoutElement.preferredHeight = STAGE_ITEM_SIZE_CUR;

            this.itemImage.sprite = this.curStageSprite;
            
        }
        else
        {
            // 깬 스테이지
            this.itemImage.sprite = this.otherStageSprite;
            //this.itemImage.color = this.clearColor;
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
        
        await SceneController.Instance.Fade(false, this.lobbyManager.FadeDuration, false);

        SceneController.Instance.AddLoadingTask(UniTask.Defer(() => DataContainer.Instance.LoadStageDatas(this.stageIndex)));

        SceneController.Instance.LoadScene(Define.Scene.Game, false).Forget();
    }

    private void PlaySoundEnterStage()
    {
        SoundManager.Instance.PlayOneShot(Define.SoundPath.SFX_ENTER_STAGE.ToString()).Forget();

        SoundManager.Instance.Stop(Define.SoundPath.BGM_LOBBY.ToString(),
            this.lobbyManager.FadeDuration);
    }
}
