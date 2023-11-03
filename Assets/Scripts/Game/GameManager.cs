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


    [SerializeField]
    private Image startTextTrans = null;




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

    public AudioClip UiSound { get; private set; }
    public AudioClip DragSound { get; private set; }

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

        var startText = this.startTextTrans.GetComponentInChildren<TextMeshProUGUI>();
        startText.text = $"Ready?";

        Initialize(this.curStageIndex);

        await SceneController.Instance.Fade(true, this.fadeDuration, false);

        GameStartFlowAsync().Forget();
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

    private async UniTaskVoid GameStartFlowAsync()
    {
        var startText = this.startTextTrans.GetComponentInChildren<TextMeshProUGUI>();

        // TODO : Delete (테스트용으로 1초 대기 걸어놓음), 추후 첫 스테이지일 경우 Tutorial 구현
        await UniTask.Delay(TimeSpan.FromMilliseconds(1000));



        SoundManager.Instance.PlayOneShot(Define.SoundPath.SFX_GAME_START.ToString()).Forget();

        // TODO : Start 글자 출력 (TODO : Localization)
        startText.text = $"Start!";
        await UniTask.Delay(TimeSpan.FromMilliseconds(500));
        _ = this.startTextTrans.DOFade(0f, 0.5f).OnComplete(() =>
        {
            this.startTextTrans.gameObject.SetActive(false);

            // 게임 시작 할 수 있는 상태로 전환
            this.isGame.Value = true;
        });
        

        Debug.Log("Game BGM 재생");
        SoundManager.Instance.Play(BgmPath, _Loop: true, _FadeTime: this.fadeDuration, _Volume: BGM_VOLUME).Forget();
    }



    private async UniTaskVoid GameEndFlowAsync()
    {
        Debug.Log("### Game End ###");

        await UniTask.Yield();

        SoundManager.Instance.Stop(DragPath);

        //if (this.seedScore.Value > 0)
        //{
        //    // TODO : Clear 연출

        //    // 씨앗을 한 개 이상 먹어야 클리어로 간주 (Heart, Fake 는 Score 안올라감)

        //    var rewardCount = CalculateReward();
        //    UserDataManager.Instance.ClearStage(rewardCount);

        //    SoundManager.Instance.PlayOneShot(Define.SoundPath.SFX_GAME_END.ToString()).Forget();
        //}
        //else
        //{
        //    // TODO : Fail 연출
        //    Debug.Log("Game FAIL! 먹은 씨앗이 없음.");

        //    SoundManager.Instance.PlayOneShot(Define.SoundPath.SFX_GAME_END_FAIL.ToString()).Forget();
        //}

        var rewardCount = CalculateReward();

        // TODO :END 팝업 표시
        if (this.resultPopup == null)
        {
            var popup = Resources.Load<GameObject>(POPUP_RESULT_PATH);

            if (popup != null)
            {
                this.resultScript = popup.GetComponent<UI_Popup_GameResult>();
                this.resultPopup = Instantiate<GameObject>(popup, this.UICanvas.transform);

                this.resultScript.Initialize(this.curStageIndex, rewardCount, this.seedScore.Value);
            }
        }
    }

    private int CalculateReward()
    {
        if (this.maxSeedCount <= REWARD_MAX)
        {
            return Math.Min(this.seedScore.Value, REWARD_MAX);
        }
        
        // 먹을 수 있는 seed 갯수가 3보다 클 땐, 3(REWARD_MAX)으로 나눠서 계산
        var oneReward = maxSeedCount / REWARD_MAX; // 총 별 3개 중 별 1개를 얻을 수 있는 씨앗 갯수
        if (this.seedScore.Value > oneReward * 2)
        {
            return REWARD_MAX;
        }
        return this.seedScore.Value > oneReward ? 2 : 1;
    }

    private void InitGameSound(string _MapName)
    {
        // Map 별로 다른 BGM 재생
        BgmPath = $"{Define.SoundPath.BGM_GAME_.ToString()}{_MapName}";

        DragPath = $"{Define.SoundPath.SFX_DRAG_.ToString()}{_MapName}";
    }

    public async void OnClick_BackAsync()
    {
        SoundManager.Instance.PlayOneShot(Define.SoundPath.SFX_BACK_BUTTON.ToString()).Forget();

        SoundManager.Instance.Stop(BgmPath, this.fadeDuration);

        await SceneController.Instance.Fade(false, this.fadeDuration, false);
        
        SceneController.Instance.LoadScene(Define.Scene.Lobby, false).Forget();
    }

    public void ResetStage()
    {
        // 먹은 씨앗 갯수 초기화
        // SeedTile 상태 초기화
        // MonsterTile 상태 초기화
        // => MapManager에서 SetMap 에서 반대로 처리하기

        // 기회는 감소한 채 그대로 => SetStage는 안해도 됨
        // 몬스터에 닿거나, 마우스 버튼 업 하면 초기화 된다는 것

        // + ReStart 하면 이 코드도 불러주고, Stage 재로드도 해야 함. Ready? 부터 다시 띄워야 함.

        // 연출

        this.seedScore.Value = 0;

        IsReset = true;

        MapManager.ResetMap();
    }
} 
