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
    public StageManager StageManager => this.stageManager;

    [SerializeField]
    private MapManager mapManager;
    public MapManager MapManager => this.mapManager;

    [SerializeField]
    private float fadeDuration = 0f;

    // Press The Screen
    [SerializeField]
    private GameObject pressScreenObj = null;




    private IReactiveProperty<bool> isGame = new ReactiveProperty<bool>(false);
    public IReactiveProperty<bool> IsGame
    {
        get => this.isGame;
        set => this.isGame = value;
    }

    [FormerlySerializedAs("seedCount")] [SerializeField]
    private IReactiveProperty<int> seedScore = new ReactiveProperty<int>();
    public IReactiveProperty<int> SeedScore
    {
        get => this.seedScore;
        set => this.seedScore = value;
    }

    private bool isReset = false;
    public bool IsReset 
    {
        get => this.isReset;
        set => this.isReset = value;
    }

    private bool isMonsterTrigger = false;
    public bool IsMonsterTrigger
    {
        get => this.isMonsterTrigger;
        set => this.isMonsterTrigger = value;
    }

    public string BgmPath { get; private set; }
    public string DragPath { get; private set; }

    private int maxSeedCount = -1;
    private int curStageIndex = -1;

    // Game Result Popup GameObject
    private GameObject resultPopup = null;
    private UI_Popup_GameResult resultScript = null;

    private const int REWARD_MAX = 3;
    private const float BGM_VOLUME = 0.3f;
    private const string POPUP_RESULT_PATH = "Prefabs/Popup_GameResult";



    private async void Start()
    {
        Debug.Log("GameManager에서 Start 진입");

        IsReset = false;

        // isGame 변수가 false가 되면 게임이 종료되었다는 것
        this.isGame
            .Skip(TimeSpan.Zero)    // 첫 프레임 호출 스킵 (시작할 때 false 로 인해 호출되는 것 방지)
            .Where(x => x == false)
            .Subscribe(_ =>
            {
                GameEndFlowAsync().Forget();
            }).AddTo(this);


        this.curStageIndex = CommonManager.Instance.CurStageIndex;
        Debug.Log($"### Current Stage Index : {this.curStageIndex}");

        Initialize(this.curStageIndex);

        await SceneController.Instance.Fade(true, this.fadeDuration, false);

        GameStartFlow();
    }

    protected override void OnDestroy()
    {
        DOTween.KillAll(true);
    }

    public void EnablePressScreen(bool _Enable)
    {
        this.pressScreenObj.SetActive(_Enable);
    }

    private void Initialize(int _CurStageIndex)
    {
        // 데이터테이블 로드
        var stageTable = DataContainer.Instance.StageTable.list[_CurStageIndex];

        InitGameSound(stageTable.MapName);

        // 스테이지 타입 받아오기
        var stageType = Enum.Parse<Define.StageType>(stageTable.StageType.Item1);

        // 해당 스테이지에서 먹을 수 있는 Seed 총 갯수 계산
        this.maxSeedCount = stageTable.SeedData.SelectMany(data =>
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
        StageManager.SetStage(_CurStageIndex + 1, stageType, stageTable.StageType.Item2, this.maxSeedCount);
        MapManager.SetMap(_CurStageIndex, DataContainer.Instance.StageSprites);
    }

    private void GameStartFlow()
    {
        var pressText = this.pressScreenObj.GetComponentInChildren<TextMeshProUGUI>();

        DOTween.Sequence()
            .Append(pressText?.transform.DOScale(1.2f, 0.3f))
            .Append(pressText?.transform.DOScale(Vector3.one, 0.3f))
            .SetLoops(-1, LoopType.Restart)
            .ToUniTask(cancellationToken: this.destroyCancellationToken).Forget();

        // 게임 시작 할 수 있는 상태로 전환
        IsGame.Value = true;
        
        Debug.Log("Game BGM 재생");
        SoundManager.Instance.Play(BgmPath, _Loop: true, _FadeTime: this.fadeDuration, _Volume: BGM_VOLUME).Forget();
    }

    


    private async UniTaskVoid GameEndFlowAsync()
    {
        Debug.Log("### Game End ###");

        await UniTask.Yield();

        SoundManager.Instance.Stop(DragPath);

        var rewardCount = CalculateReward(this.seedScore.Value);

        if (rewardCount > 0)
        {
            UserDataManager.Instance.ClearStage(this.curStageIndex, rewardCount);
        }
        
        if (this.resultPopup == null)
        {
            var popup = Resources.Load<GameObject>(POPUP_RESULT_PATH);

            if (popup != null)
            {
                this.resultPopup = Instantiate<GameObject>(popup, this.UICanvas.transform);
                this.resultScript = this.resultPopup.GetComponent<UI_Popup_GameResult>();
            }
        }

        this.resultScript?.Initialize(this.curStageIndex, rewardCount, this.seedScore.Value).Forget();
    }

    private int CalculateReward(int value)
    {
        if (value == 0)
        {
            // Not Clear
            return 0;
        }

        // 먹을 수 있는 씨앗 갯수가 3보다 작거나 같을 때
        if (this.maxSeedCount <= REWARD_MAX)
        {
            if (value == this.maxSeedCount)
            {
                return REWARD_MAX;
            }

            return Math.Min(value, this.maxSeedCount);
        }
        
        // 먹을 수 있는 seed 갯수가 3보다 클 땐, 3(REWARD_MAX)으로 나눠서 계산
        var oneReward = maxSeedCount / REWARD_MAX; // 총 별 3개 중 별 1개를 얻을 수 있는 씨앗 갯수
        if (value > oneReward * 2)
        {
            return REWARD_MAX;
        }
        return value > oneReward ? 2 : 1;
    }

    private void InitGameSound(string _MapName)
    {
        // Map 별로 다른 BGM 재생
        BgmPath = $"{Define.SoundPath.BGM_GAME_.ToString()}{_MapName}";

        DragPath = $"{Define.SoundPath.SFX_DRAG_.ToString()}{_MapName}";
    }

    private bool RaycastGameScreen(Vector3 _MousePosition)
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(_MousePosition);

        var lineLayer = (1 << LayerMask.NameToLayer("GameScreen"));
        var raycastResult = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, lineLayer);

        return raycastResult.collider != null ? true : false;
    }

    public void OnClick_PressScreen()
    {
        Debug.Log("눌러따");

        if (RaycastGameScreen(Input.mousePosition) == true)
        {
            EnablePressScreen(false);
        }
    }

    public async void OnClick_BackAsync()
    {
        SoundManager.Instance.PlayOneShot(Define.SoundPath.SFX_BACK_BUTTON.ToString()).Forget();

        SoundManager.Instance.Stop(BgmPath, this.fadeDuration);

        await SceneController.Instance.Fade(false, this.fadeDuration, false);
        
        SceneController.Instance.LoadScene(Define.Scene.Lobby, false).Forget();
    }

    

    public void RewindStage()
    {
        // 스테이지 처음 상태로 되감기

        // 먹은 씨앗 갯수 초기화
        // SeedTile 상태 초기화
        // MonsterTile 상태 초기화

        Debug.Log("### Rewind Stage ###");

        this.seedScore.Value = 0;

        IsReset = true;

        IsMonsterTrigger = false;

        MapManager.ResetMap();

        EnablePressScreen(true);
    }
} 
