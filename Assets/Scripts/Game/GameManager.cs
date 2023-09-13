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

        var stageType = (Define.StageType)Enum.Parse(typeof(Define.StageType), stageTable.StageType.Item1);
        
        // 해당 스테이지에서 먹을 수 있는 Seed 총 갯수 계산
        var maxSeedCount = stageTable.SeedData.SelectMany(data => Enumerable.Repeat(1, data.Item3)).Sum();

        StageManager.SetStage(curStageIndex + 1, stageType, stageTable.StageType.Item2, maxSeedCount);
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
        MapManager.IsFade.Value = false;

        await UniTask.Delay(TimeSpan.FromMilliseconds(3000));
        
        // TODO :END 팝업 표시
    }

    public async void OnClick_Back()
    {
        await SceneController.Instance.Fade(false, this.fadeDuration, false, new CancellationTokenSource());
        
        SceneController.Instance.LoadScene(Define.Scene.Lobby, false).Forget();
    }
}
