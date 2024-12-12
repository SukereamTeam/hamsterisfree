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
        itemImage = GetComponent<Image>();
        if (itemImage == null)
        {
            Debug.Log("### Not Found StageItem Image Component. ###");
        }
    }

    private void ScrollCellIndex(int index)
    {
        if (lobbyManager == null)
        {
            lobbyManager = LobbyManager.Instance;
        }
        
        layoutElement.preferredWidth = STAGE_ITEM_SIZE;
        layoutElement.preferredHeight = STAGE_ITEM_SIZE;
        
        stageIndex = index;

        stageItemText.text = $"{index + 1}";

        itemImage.color = Color.white;

        if (lockObject.activeSelf == true)
        {
            lockObject.SetActive(false);
        }

        itemImage.color = Color.white;
        
        if (UserDataManager.Instance.CurUserData.CurrentStage < stageIndex)
        {
            // 현재 깰 수 없는 스테이지 (남은 스테이지)
            lockObject.SetActive(true);
            
            itemImage.sprite = otherStageSprite;
            itemImage.color = Color.grey;
        }
        else if (UserDataManager.Instance.CurUserData.CurrentStage == stageIndex)
        {
            // 현재 깨야 하는 스테이지
            layoutElement.preferredWidth = STAGE_ITEM_SIZE_CUR;
            layoutElement.preferredHeight = STAGE_ITEM_SIZE_CUR;

            itemImage.sprite = curStageSprite;
            
        }
        else
        {
            // 깬 스테이지
            itemImage.sprite = otherStageSprite;
        }
    }

    public async void OnClick_ItemAsync()
    {
        if (UserDataManager.Instance.CurUserData.CurrentStage < stageIndex)
        {
            Debug.Log("아직 이전 스테이지 클리어 안 함!");
            return;
        }

        PlaySoundEnterStage();

        CommonManager.Instance.CurStageIndex = stageIndex;
        
        await SceneController.Instance.Fade(false, lobbyManager.FadeDuration, false);

        SceneController.Instance.AddLoadingTask(UniTask.Defer(() => DataContainer.Instance.LoadStageDatas(stageIndex)));

        SceneController.Instance.LoadScene(Define.Scene.Game, false).Forget();
    }

    private void PlaySoundEnterStage()
    {
        SoundManager.Instance.PlayOneShot(Define.SoundPath.SFX_ENTER_STAGE.ToString()).Forget();

        SoundManager.Instance.Stop(Define.SoundPath.BGM_LOBBY.ToString(),
            lobbyManager.FadeDuration);
    }
}
