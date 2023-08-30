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
    private int seedScore;
    public int SeedScore
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
                GameEndFlow();
            }).AddTo(this);


        var curStageIndex = CommonManager.Instance.CurStageIndex;
        Debug.Log($"### Current Stage Index : {curStageIndex}");
        
        // 데이터테이블 로드
        var stageTable = DataContainer.Instance.StageTable.list[curStageIndex];

        var stageType = (Define.StageType)Enum.Parse(typeof(Define.StageType), stageTable.StageType.Type);
        var maxSeedCount = stageTable.SeedData.SelectMany(data => Enumerable.Repeat(1, data.Value)).Sum();

        StageManager.SetStage(curStageIndex + 1, stageType, stageTable.StageType.Value, maxSeedCount);
        MapManager.SetMap(curStageIndex, DataContainer.Instance.StageSprites);

        await SceneController.Instance.Fade(true, this.fadeDuration, false, new CancellationTokenSource());

        // TODO : Delete (테스트용으로 5초 대기 걸어놓음)
        await UniTask.Delay(TimeSpan.FromMilliseconds(5000));

        // 게임 시작 할 수 있는 상태로 전환
        this.isGame.Value = true;
    }




    private void GameEndFlow()
    {
        Debug.Log("### Game End ###");

        // TODO
        // 지금은 이걸로 페이드 없애버리지만 나중엔 애니 효과든 뭐든 넣어야 함
        MapManager.IsFade.Value = false;
    }

    public async void OnClick_Back()
    {
        await SceneController.Instance.Fade(false, this.fadeDuration, false, new CancellationTokenSource());
        
        SceneController.Instance.LoadScene(Define.Scene.Lobby, false).Forget();
        
        Clear();
    }
}
