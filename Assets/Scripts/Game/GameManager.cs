using System;
using System.Linq;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.Serialization;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class GameManager : MonoSingleton<GameManager>
{
    [SerializeField]
    private Canvas UICanvas = null;

    [SerializeField]
    private StageManager stageManager;
    public StageManager StageManager => stageManager;

    [SerializeField]
    private MapManager mapManager;
    public MapManager MapManager => mapManager;

    [SerializeField]
    private float fadeDuration = 0f;

    // Press The Screen
    [SerializeField]
    private GameObject pressScreenObj = null;




    public IReactiveProperty<bool> IsGame = new ReactiveProperty<bool>(false);

    [FormerlySerializedAs("seedCount")] [SerializeField]
    private IReactiveProperty<int> seedScore = new ReactiveProperty<int>();
    public IReactiveProperty<int> SeedScore
    {
        get => seedScore;
        set => seedScore = value;
    }

    private bool isReset = false;
    public bool IsReset 
    {
        get => isReset;
        set => isReset = value;
    }

    private bool isMonsterTrigger = false;
    public bool IsMonsterTrigger
    {
        get => isMonsterTrigger;
        set => isMonsterTrigger = value;
    }

    public string BgmPath { get; private set; }
    public string DragPath { get; private set; }

    private int _maxSeedCount = -1;
    private int _curStageIndex = -1;

    // Game Result Popup GameObject
    private GameObject _resultPopup = null;
    private UI_Popup_GameResult _resultScript = null;

    private const int REWARD_MAX = 3;
    private const float BGM_VOLUME = 0.3f;
    private const string POPUP_RESULT_PATH = "Prefabs/Popup_GameResult";



    private async void Start()
    {
        Debug.Log("GameManager에서 Start 진입");

        IsReset = false;

        // isGame 변수가 false가 되면 게임이 종료되었다는 것
        IsGame
            .Skip(TimeSpan.Zero)    // 첫 프레임 호출 스킵 (시작할 때 false 로 인해 호출되는 것 방지)
            .Where(x => x == false)
            .Subscribe(_ =>
            {
                GameEndFlowAsync().Forget();
            }).AddTo(this);


        _curStageIndex = CommonManager.Instance.CurStageIndex;
        Debug.Log($"### Current Stage Index : {_curStageIndex}");

        Initialize(_curStageIndex);

        await SceneController.Instance.Fade(true, fadeDuration, false);

        GameStartFlow();
    }

    protected override void OnDestroy()
    {
        DOTween.KillAll(true);
    }

    public void EnablePressScreen(bool isActive)
    {
        pressScreenObj.SetActive(isActive);
    }

    private void Initialize(int curStageIndex)
    {
        // 데이터테이블 로드
        var stageTable = DataContainer.Instance.StageTable.list[curStageIndex];

        InitGameSound(stageTable.MapName);

        // 스테이지 타입 받아오기
        var stageType = Enum.Parse<Define.StageType>(stageTable.StageType.Item1);

        // 해당 스테이지에서 먹을 수 있는 Seed 총 갯수 계산
        _maxSeedCount = stageTable.SeedData.SelectMany(data =>
        {
            var targetSubType = data.Item1;     // eg. Default
            var targetSubTypeIndex = data.Item2;  // eg. Default_01 <- '01'
            var targetCount = data.Item3;         // 해당 타입 타일의 갯수

            var subType = Enum.Parse<Define.TileType_Sub>(targetSubType);
            if (subType != Define.TileType_Sub.Heart && subType != Define.TileType_Sub.Fake)
            {
                // targetCount 값 만큼의 1로 이루어진 시퀀스를 반환
                // eg. Default Type의 Tile이 3개 있을 경우 : 3 x 1 = 3
                return Enumerable.Repeat(1, targetCount);
            }
            else
            {
                return Enumerable.Empty<int>();
            }
        }).Sum();

        // 테이블 토대로 세팅
        StageManager.SetStage(curStageIndex + 1, stageType, stageTable.StageType.Item2, _maxSeedCount);
        MapManager.SetMap(curStageIndex, DataContainer.Instance.StageSprites);
    }

    private void GameStartFlow()
    {
        var pressText = pressScreenObj.GetComponentInChildren<TextMeshProUGUI>();

        DOTween.Sequence()
            .Append(pressText?.transform.DOScale(1.2f, 0.3f))
            .Append(pressText?.transform.DOScale(Vector3.one, 0.3f))
            .SetLoops(-1, LoopType.Restart)
            .ToUniTask(cancellationToken: destroyCancellationToken).Forget();

        // 게임 시작 할 수 있는 상태로 전환
        IsGame.Value = true;
        
        Debug.Log("Game BGM 재생");
        SoundManager.Instance.Play(BgmPath, _Loop: true, _FadeTime: fadeDuration, _Volume: BGM_VOLUME).Forget();
    }

    


    private async UniTaskVoid GameEndFlowAsync()
    {
        Debug.Log("### Game End ###");

        await UniTask.Yield();

        SoundManager.Instance.Stop(DragPath);

        var rewardCount = CalculateReward(seedScore.Value);

        if (rewardCount > 0)
        {
            UserDataManager.Instance.ClearStage(_curStageIndex, rewardCount);
        }
        
        if (_resultPopup == null)
        {
            var popup = Resources.Load<GameObject>(POPUP_RESULT_PATH);

            if (popup != null)
            {
                _resultPopup = Instantiate<GameObject>(popup, UICanvas.transform);
                _resultScript = _resultPopup.GetComponent<UI_Popup_GameResult>();
            }
        }

        _resultScript?.Initialize(_curStageIndex, rewardCount, seedScore.Value).Forget();
    }

    private int CalculateReward(int value)
    {
        if (value == 0)
        {
            // Not Clear
            return 0;
        }

        // 먹을 수 있는 씨앗 갯수가 3보다 작거나 같을 때
        if (_maxSeedCount <= REWARD_MAX)
        {
            if (value == _maxSeedCount)
            {
                return REWARD_MAX;
            }

            return Math.Min(value, _maxSeedCount);
        }
        
        // 먹을 수 있는 seed 갯수가 3보다 클 땐, 3(REWARD_MAX)으로 나눠서 계산
        var oneReward = _maxSeedCount / REWARD_MAX; // 총 별 3개 중 별 1개를 얻을 수 있는 씨앗 갯수
        if (value > oneReward * 2)
        {
            return REWARD_MAX;
        }
        return value > oneReward ? 2 : 1;
    }

    private void InitGameSound(string mapName)
    {
        // Map 별로 다른 BGM 재생
        BgmPath = $"{Define.SoundPath.BGM_GAME_.ToString()}{mapName}";

        DragPath = $"{Define.SoundPath.SFX_DRAG_.ToString()}{mapName}";
    }

    private bool RaycastGameScreen(Vector3 mousePosition)
    {
	    if (Camera.main != null)
	    {
		    Vector2 position = Camera.main.ScreenToWorldPoint(mousePosition);

		    var lineLayer = (1 << LayerMask.NameToLayer("GameScreen"));
		    var result = Physics2D.Raycast(position, Vector2.zero, Mathf.Infinity, lineLayer);

		    return result.collider != null;
	    }

	    return false;
    }

    public void OnClick_PressScreen()
    {
        if (RaycastGameScreen(Input.mousePosition) == true)
        {
            EnablePressScreen(false);
        }
    }

    public async void OnClick_BackAsync()
    {
        SoundManager.Instance.PlayOneShot(Define.SoundPath.SFX_BACK_BUTTON.ToString()).Forget();

        SoundManager.Instance.Stop(BgmPath, fadeDuration);

        await SceneController.Instance.Fade(false, fadeDuration, false);
        
        SceneController.Instance.LoadScene(Define.Scene.Lobby, false).Forget();
    }

    

    public void RewindStage()
    {
        // 스테이지 처음 상태로 되감기

        // 먹은 씨앗 갯수 초기화
        // SeedTile 상태 초기화
        // MonsterTile 상태 초기화

        Debug.Log("### Rewind Stage ###");

        seedScore.Value = 0;

        IsReset = true;

        IsMonsterTrigger = false;

        MapManager.ResetMap();

        EnablePressScreen(true);
    }
} 
