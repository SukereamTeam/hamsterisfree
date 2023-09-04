using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UniRx;
using DG.Tweening;
using DataTable;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MapManager : MonoBehaviour
{
    // 맵 생성
    [FormerlySerializedAs("backgroundRenderer")] [SerializeField]
    private SpriteRenderer tileBackRenderer;

    [SerializeField]
    private Transform[] backTiles;

    [SerializeField]
    private Transform[] outlineTiles;

    [SerializeField]
    private SpriteRenderer[] edgeTiles;

    [SerializeField] private GameObject mask;
    public GameObject Mask => this.mask;
    
    [FormerlySerializedAs("mask")] [SerializeField]
    private SpriteRenderer blockRenderer;
    public SpriteRenderer BlockRenderer => this.blockRenderer;

    [SerializeField]
    private Image backgroundImage;

    [SerializeField]
    private float fadeTime = 0f;
    public float FadeTime => this.fadeTime;

    [SerializeField]
    private Transform tileRoot;

    [SerializeField]
    private GameObject exitPrefab;

    [SerializeField]
    private GameObject seedPrefab;
    
    [SerializeField]
    private GameObject monsterPrefab;
    



    private const int Left_End = 9;
    private const int Bottom_End = 15;
    private const int Right_End = 24;


    private IReactiveProperty<bool> isFade = new ReactiveProperty<bool>(false);
    public IReactiveProperty<bool> IsFade
    {
        get => this.isFade;
        set => this.isFade = value;
    }

    private List<SeedTile> seedTiles;
    private List<MonsterTile> monsterTiles;
    public IReadOnlyList<SeedTile> SeedTiles => this.seedTiles;

    private int randomSeed = 0;




    private void Awake()
    {
        // 하이어라키의 타일 오브젝트들 보기 편하도록 이름 바꿔주기
        ChangeNameOutlineTiles();

        this.backTiles = this.tileBackRenderer.transform.parent.GetComponentsInChildren<SpriteRenderer>()
            .Select(x => x.transform)
            .Where(x => x != this.tileBackRenderer.transform)
            .ToArray();

        this.seedTiles = new List<SeedTile>();
        this.monsterTiles = new List<MonsterTile>();

        this.randomSeed = (int)DateTime.Now.Ticks;
        UnityEngine.Random.InitState(this.randomSeed);
    }

    private void Start()
    {
        this.isFade
            .Skip(TimeSpan.Zero)    // 첫 프레임 호출 스킵 (시작할 때 false 로 인해 호출되는 것 방지)
            .Subscribe(_ =>
            {
                FadeMap();
            }).AddTo(this);


        
    }

    public void SetMap(int _CurStage, IReadOnlyList<Sprite> _StageSprites)
    {
        if (_StageSprites.Count == 0)
        {
            Debug.Log("### Error ---> DataContainer.StageTileSprites.Count == 0 ###");
            return;
        }

        SetBackground(_StageSprites);
        SetOutlineTiles(_StageSprites);
        SetMask(_StageSprites);

        for (int i = 0; i < Enum.GetValues(typeof(Define.TileType)).Length; i++)
        {
            if (i == (int)Define.TileType.Exit)
            {
                CreateExitTile(Define.TileType.Exit);
            }
            else if (i == (int)Define.TileType.Seed)
            {
                CreateSeedTile(_CurStage, Define.TileType.Seed);
            }
            else if (i == (int)Define.TileType.Monster)
            {
                CreateMonsterTile(_CurStage, Define.TileType.Monster);
            }
        }
    }

    //------------------ Create Tiles

    private void CreateExitTile(Define.TileType _TileType)
    {
        var random = UnityEngine.Random.Range(0, outlineTiles.Length);
        var randomPos = new Vector2(outlineTiles[random].transform.localPosition.x, outlineTiles[random].transform.localPosition.y);

        var exitTile = Instantiate<GameObject>(this.exitPrefab, this.tileRoot);
        var exitScript = exitTile.GetComponent<ExitTile>();

        TileBase.TileInfo baseInfo = new TileBase.TileInfo
        {
            Type = _TileType,
            RootIdx = random
        };

        TileBase.TileInfo tileInfo = new TileBase.TileBuilder(baseInfo).Build();

        exitScript.Initialize(tileInfo, randomPos);

        // TODO
        // 하위에 탈출 셰이더(빛 효과) 메테리얼 오브젝트 추가
        // 타일 좌표에 따라 메테리얼 오브젝트 방향 바꿔줘야 함 (x > 0 ? shader 오른쪽에서 뻗어나오고 : 왼쪽에서 뻗어나오고 y > 0 ? 위에서 뻗어나오고 : 아래에서 뻗어나오고)
    }

    private void CreateSeedTile(int _CurStage, Define.TileType _TileType)
    {
        var stageTable = DataContainer.Instance.StageTable.list[_CurStage];

        var posList = GetRandomPosList(Define.TileType.Seed, stageTable.SeedData);
        int posIdx = 0;

        for (int i = 0; i < stageTable.SeedData.Count; i++)
        {
            for (int j = 0; j < stageTable.SeedData[i].Item3; j++)
            {
                var randomPos = new Vector2(posList[posIdx].transform.position.x, posList[posIdx].transform.position.y);
                
                var seedTile = Instantiate<GameObject>(seedPrefab, this.tileRoot);
                var seedScript = seedTile.GetComponent<SeedTile>();

                // eg. SeedTile 의 타입들 중 Default_0 타입에 대한 데이터를 SeedTable에서 가져오기
                var targetSeedData = DataContainer.Instance.SeedTable.GetParamFromType(stageTable.SeedData[i].Item1, stageTable.SeedData[i].Item2);

                // 기본 정보 초기화
                TileBase.TileInfo baseInfo = new TileBase.TileInfo
                {
                    Type = _TileType,
                    RootIdx = posList[posIdx].root
                };

                // 추가 정보 더해서 초기화 (SubType, ActiveTime)
                TileBase.TileInfo tileInfo = new TileBase.TileBuilder(baseInfo)
                    .WithSubType(targetSeedData.Type)
                    .WithSubTypeIndex(targetSeedData.TypeIndex)
                    .WithActiveTime(targetSeedData.ActiveTime)
                    .WithSeedValue(targetSeedData.SeedValue)
                    .Build();

                seedScript.Initialize(tileInfo, randomPos);

                this.seedTiles.Add(seedScript);

                posIdx++;
            }
        }
    }
    
    private void CreateMonsterTile(int _CurStage, Define.TileType _TileType)
    {
        // MonsterTile은 위아래 혹은 양 옆으로 왔다갔다 한다.
        // 그러므로 위아래로 움직이게 될 경우 x좌표만 필요하고, 양 옆으로 움직일 경우 y좌표만 필요하다.
        // 위아래가 될지 양 옆이 될지 랜덤으로 정한 뒤
        // 각 필요한 좌표를 랜덤으로 지정한다.
        
        var stageTable = DataContainer.Instance.StageTable.list[_CurStage];

        var posList = GetRandomPosList(Define.TileType.Monster, stageTable.MonsterData);
        int posIdx = 0;

        for (int i = 0; i < stageTable.MonsterData.Count; i++)
        {
            for (int j = 0; j < stageTable.MonsterData[i].Item3; j++)
            {
                var randomPos = new Vector2(posList[posIdx].transform.position.x, posList[posIdx].transform.position.y);
                
                var monsterTile = Instantiate<GameObject>(monsterPrefab, this.tileRoot);
                var monsterScript = monsterTile.GetComponent<MonsterTile>();

                // eg. MonsterTile 의 타입들 중 Default_0 타입에 대한 데이터를 MonsterTable에서 가져오기
                var targetMonsterData = DataContainer.Instance.MonsterTable.GetParamFromType(stageTable.MonsterData[i].Item1, stageTable.MonsterData[i].Item2);

                // 기본 정보 초기화
                TileBase.TileInfo baseInfo = new TileBase.TileInfo
                {
                    Type = _TileType,
                    RootIdx = posList[posIdx].root
                };

                // 추가 정보 더해서 초기화 (SubType, ActiveTime)
                TileBase.TileInfo tileInfo = new TileBase.TileBuilder(baseInfo)
                    .WithSubType(targetMonsterData.Type)
                    .WithActiveTime(targetMonsterData.ActiveTime)
                    .WithSubTypeIndex(targetMonsterData.TypeIndex)
                    .Build();

                monsterScript.Initialize(tileInfo, randomPos);

                this.monsterTiles.Add(monsterScript);

                posIdx++;
            }
        }
    }

    /// <summary>
    /// Random Pos가 필요한 타일 리스트를 매개변수로 넣어주면
    /// 리스트의 타일들 갯수만큼 랜덤Pos 생성하여 List에 담아 반환
    /// </summary>
    /// <returns>Transform은 Pos값을 위해, int는 backTiles중 어느 타일을 참조했는지 파악하려고</returns>
    private List<(Transform transform, int root)> GetRandomPosList(Define.TileType _TileType, List<Table_Base.SerializableTuple<string, int, int>> _List)
    {
        // 랜덤 포지션이 필요한 타일 갯수 구하기 (타일 타입별로 Count 더해주기)
        var randomCount = _List.Select(x => x.Item3).Sum();
        
        var resultTile = new List<(Transform transform, int root)>();

        Transform[] targetArray = new Transform[] { };
        
        if (_TileType == Define.TileType.Monster)
        {
            targetArray = this.backTiles
                .Where(tile => 
                    (tile.position.x >= 1 && tile.position.x <= 6 && (tile.position.y == 0 || tile.position.y == 8)) ||
                    ((tile.position.x == 1 || tile.position.x == 6) && (tile.position.y >= 0 && tile.position.y <= 8))
                )
                .ToArray();
        }
        else if (_TileType == Define.TileType.Seed)
        {
            targetArray = this.backTiles;
        }
        
        
        // 기존에 멤버변수로 갖고있던 backTiles 참조해서 포지션 List 만듦
        var targetTiles = new List<Transform>(targetArray);

        
        
        for (int i = 0; i < randomCount; i++)
        {
            var random = UnityEngine.Random.Range(0, targetTiles.Count);

            // 참조한 타일이 어느 타일인지
            int index = 0;

            index = Array.FindIndex(this.backTiles, x => x == targetTiles[random]);
            
            resultTile.Add((transform: targetTiles[random], root: index));

            targetTiles.RemoveAt(random);
        }

        return resultTile;
    }

    public (int rootIdx, Vector2 pos) GetRandomPosition_Next(Define.TileType _TileType)
    {
        int rootIdx = -1;
        Vector2 pos = Vector2.zero;


        switch(_TileType)
        {
            case Define.TileType.Seed:
                {
                    // Seed랑 Monster는 backTiles를 참조하여 타일들을 만듦 (Exit는 outlineTiles 참조)
                    var targetTiles = new List<Transform>(this.backTiles);

                    var seedTilesRoot = this.seedTiles.Select(x => x.Info.RootIdx).ToList();

                    // targetTiles 리스트를 순회하면서
                    // seedTiles 리스트의 RootIdx와 같은 인덱스를 가진 요소는 제외한 리스트 생성
                    // 다음 RandomPos를 뽑을 Pool이 될 것임
                    var tilePool = targetTiles
                        .Where((x, index) => seedTilesRoot.Contains(index) == false)
                        .ToList();

                    var random = UnityEngine.Random.Range(0, tilePool.Count);
                    var randomPos = new Vector2(tilePool[random].position.x, tilePool[random].position.y);

                    // 참조한 타일이 어느 타일인지
                    int index = Array.FindIndex(this.backTiles, x => x == tilePool[random]);

                    rootIdx = index;
                    pos = randomPos;
                }
                break;
            case Define.TileType.Monster:
            {
                var targetTiles = this.backTiles.Where(tile => 
                        (tile.position.x >= 1 && tile.position.x <= 6 && (tile.position.y == 0 || tile.position.y == 8)) ||
                        ((tile.position.x == 1 || tile.position.x == 6) && (tile.position.y >= 0 && tile.position.y <= 8))
                    )
                    .ToArray();
                
                var monsterTilesRoot = this.monsterTiles.Select(x => x.Info.RootIdx).ToList();

                var tilePool = targetTiles
                    .Where((x, index) => monsterTilesRoot.Contains(index) == false)
                    .ToList();
                
                var random = UnityEngine.Random.Range(0, tilePool.Count);
                var randomPos = new Vector2(tilePool[random].position.x, tilePool[random].position.y);

                // 참조한 타일이 어느 타일인지
                int index = Array.FindIndex(this.backTiles, x => x == tilePool[random]);

                rootIdx = index;
                pos = randomPos;
            }
                break;
        }


        return (rootIdx, pos);
    }

    
    //------------------


    private void FadeMap()
    {
        Debug.Log($"FadeMap / isFade : {this.isFade.Value}");

        if (this.isFade.Value == true)
        {
            this.blockRenderer.DOFade(1f, fadeTime).OnComplete(() => Debug.Log("### Fade Complete ###"));
        }
        else
        {
            this.blockRenderer.DOKill(true);

            this.blockRenderer.color = new Color(1f, 1f, 1f, 0f);
        }
    }



    //------------ Setting Stage Data

    private void SetBackground(IReadOnlyList<Sprite> _StageSprites)
    {
        var index = (int)Define.TileSpriteName.Center;

        this.tileBackRenderer.sprite = _StageSprites[index];

        index = (int)Define.TileSpriteName.Background;

        this.backgroundImage.sprite = _StageSprites[index];
    }

    private void SetOutlineTiles(IReadOnlyList<Sprite> _StageSprites)
    {
        var index = (int)Define.TileSpriteName.Center;

        for (int i = 0; i < outlineTiles.Length; i++)
        {
            if (i < Left_End)
            {
                index = (int)Define.TileSpriteName.Left;
            }
            else if (i < Bottom_End)
            {
                //bottom
                index = (int)Define.TileSpriteName.Bottom;
            }
            else if (i < Right_End)
            {
                //right
                index = (int)Define.TileSpriteName.Right;
            }
            else
            {
                //top
                index = (int)Define.TileSpriteName.Top;
            }
            
            var sprite = _StageSprites[index];

            var renderer = outlineTiles[i].GetChild(0).GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.sprite = sprite;
            }
        }


        for (index = (int)Define.TileSpriteName.TopLeft; index <= (int)Define.TileSpriteName.BottomRight; index++)
        {
            var tileIndex = index - (int)Define.TileSpriteName.TopLeft;
            this.edgeTiles[tileIndex].sprite = _StageSprites[index];
        }
    }


    private void SetMask(IReadOnlyList<Sprite> _StageSprites)
    {
        Debug.Log("SetMask 시작");
        var sprite = _StageSprites[(int)Define.TileSpriteName.Mask];
        
        this.blockRenderer.sprite = sprite;
        Debug.Log("SetMask 끝");
    }


    private void ChangeNameOutlineTiles()
    {
        for (int i = 0; i < outlineTiles.Length; i++)
        {
            outlineTiles[i].name = $"Tile ({outlineTiles[i].localPosition.x}, {outlineTiles[i].localPosition.y})";
        }
    }
}