using System;
using System.Linq;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.Serialization;

public class GameManager : MonoSingleton<GameManager>
{
    [SerializeField] private StageManager stageManager;
    public StageManager StageManager => this.stageManager;

    [SerializeField]
    private MapManager mapManager;
    public MapManager MapManager => this.mapManager;

    [SerializeField]
    private float fadeDuration = 0f;



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

    private int maxSeedCount = -1;

    private const int REWARD_MAX = 3;




    private async void Start()
    {
        Debug.Log("GameManager에서 Start 진입");

        // isGame 변수가 false가 되면 게임이 종료되었다는 것
        this.isGame
            .Skip(TimeSpan.Zero)    // 첫 프레임 호출 스킵 (시작할 때 false 로 인해 호출되는 것 방지)
            .Where(x => x == false)
            .Subscribe(_ =>
            {
                GameEndFlow().Forget();
            }).AddTo(this);


        var curStageIndex = CommonManager.Instance.CurStageIndex;
        Debug.Log($"### Current Stage Index : {curStageIndex}");
        
        // 데이터테이블 로드
        var stageTable = DataContainer.Instance.StageTable.list[curStageIndex];

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

        StageManager.SetStage(curStageIndex + 1, stageType, stageTable.StageType.Item2, this.maxSeedCount);
        MapManager.SetMap(curStageIndex, DataContainer.Instance.StageSprites);

        await SceneController.Instance.Fade(true, this.fadeDuration, false, new CancellationTokenSource());

        // TODO : Delete (테스트용으로 5초 대기 걸어놓음), 추후 첫 게임일 경우 Tutorial 구현
        await UniTask.Delay(TimeSpan.FromMilliseconds(3000));

        // 게임 시작 할 수 있는 상태로 전환
        this.isGame.Value = true;
    }




    private async UniTaskVoid GameEndFlow()
    {
        Debug.Log("### Game End ###");

        // TODO
        // 지금은 이걸로 페이드 없애버리지만 나중엔 애니 효과든 뭐든 넣어야 함

        await UniTask.Yield();
        
        // TODO :END 팝업 표시
        
        if (this.seedScore.Value > 0)
        {
            // TODO : Clear 연출
            
            // 씨앗을 한 개 이상 먹어야 클리어로 간주 (Heart, Fake 는 Score 안올라감)
            
            var rewardCount = CalculateReward();
            UserDataManager.Instance.ClearStage(rewardCount);
        }
        else
        {
            // TODO : Fail 연출
            Debug.Log("Game FAIL! 먹은 씨앗이 없음.");
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

    public async void OnClick_BackAsync()
    {
        await SceneController.Instance.Fade(false, this.fadeDuration, false, new CancellationTokenSource());
        
        SceneController.Instance.LoadScene(Define.Scene.Lobby, false).Forget();
    }
}
