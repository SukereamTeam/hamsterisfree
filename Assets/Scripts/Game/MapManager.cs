using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UniRx;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using DataTable;

public class MapManager : MonoBehaviour
{
    // 맵 생성
    [SerializeField]
    private SpriteRenderer backgroundRenderer;
    public SpriteRenderer BackgroundRenderer => this.backgroundRenderer;

    [SerializeField]
    private Transform[] backTiles;

    [SerializeField]
    private Transform[] outlineTiles;

    [SerializeField]
    private SpriteRenderer[] edgeTiles;

    [SerializeField]
    private SpriteRenderer mask;
    public SpriteRenderer Mask => this.mask;

    [SerializeField]
    private float fadeTime = 0f;
    public float FadeTime => this.fadeTime;

    [SerializeField]
    private Transform tileRoot;

    [SerializeField]
    private GameObject exitPrefab;

    [SerializeField]
    private GameObject seedPrefab;



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

    private int randomSeed = 0;




    private void Awake()
    {
        // 하이어라키의 타일 오브젝트들 보기 편하도록 이름 바꿔주기
        ChangeNameOutlineTiles();

        this.backTiles = this.backgroundRenderer.transform.parent.GetComponentsInChildren<SpriteRenderer>()
            .Select(x => x.transform)
            .Where(x => x != this.backgroundRenderer.transform)
            .ToArray();

        this.seedTiles = new List<SeedTile>();

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

    public void SetStage(int _CurStage, IReadOnlyList<Sprite> _StageSprites)
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
            Pos = randomPos,
        };

        TileBase.TileInfo tileInfo = new TileBase.TileBuilder(baseInfo).Build();

        exitScript.Initialize(tileInfo);

        // TODO
        // 하위에 탈출 셰이더(빛 효과) 메테리얼 오브젝트 추가
        // 타일 좌표에 따라 메테리얼 오브젝트 방향 바꿔줘야 함 (x > 0 ? shader 오른쪽에서 뻗어나오고 : 왼쪽에서 뻗어나오고 y > 0 ? 위에서 뻗어나오고 : 아래에서 뻗어나오고)
    }

    private void CreateSeedTile(int _CurStage, Define.TileType _TileType)
    {
        var stageTable = DataContainer.Instance.StageTable.list[_CurStage];

        var posList = GetRandomPosList(stageTable.SeedData);
        int k = 0;

        for (int i = 0; i < stageTable.SeedData.Count; i++)
        {
            for (int j = 0; j < stageTable.SeedData[i].Count; j++)
            {
                var randomPos = new Vector2(posList[k].position.x, posList[k].position.y);
                k++;

                var seedTile = Instantiate<GameObject>(seedPrefab, this.tileRoot);
                var seedScript = seedTile.GetComponent<SeedTile>();

                // eg. SeedTile 의 타입들 중 Default 타입에 대한 데이터를 SeedTable에서 가져오기
                var targetSeedData = DataContainer.Instance.SeedTable.GetParamFromType(stageTable.SeedData[i].Type);

                // 기본 정보 초기화
                TileBase.TileInfo baseInfo = new TileBase.TileInfo
                {
                    Type = _TileType,
                    Pos = randomPos,
                };

                // 추가 정보 더해서 초기화 (SubType, ActiveTime)
                TileBase.TileInfo tileInfo = new TileBase.TileBuilder(baseInfo)
                    .WithSubType(targetSeedData.Type)
                    .WithActiveTime(targetSeedData.ActiveTime)
                    .Build();

                seedScript.Initialize(tileInfo);

                this.seedTiles.Add(seedScript);
            }
        }
    }

    /// <summary>
    /// Random Pos가 필요한 타일 리스트를 매개변수로 넣어주면
    /// 리스트의 타일들 갯수만큼 랜덤Pos 생성하여 List에 담아 반환
    /// </summary>
    private List<Transform> GetRandomPosList(List<Table_Base.SerializableTuple<string, int>> _List)
    {
        // 랜덤 포지션이 필요한 타일 갯수 구하기 (타일 타입별로 Count 더해주기)
        var randomCount = _List.Select(x => x.Count).Sum();

        // 기존에 멤버변수로 갖고있던 backTiles 참조해서 포지션 List 만듦
        var targetTiles = new List<Transform>(this.backTiles);

        var resultTile = new List<Transform>(randomCount);

        for (int i = 0; i < randomCount; i++)
        {
            var random = UnityEngine.Random.Range(0, targetTiles.Count);

            resultTile.Add(targetTiles[random]);

            targetTiles.RemoveAt(random);
        }

        return resultTile;
    }

    //------------------



    private void FadeMap()
    {
        Debug.Log($"FadeMap / isFade : {this.isFade.Value}");

        if (this.isFade.Value == true)
        {
            this.mask.DOFade(1f, fadeTime).OnComplete(() => Debug.Log("### Fade Complete ###"));
        }
        else
        {
            this.mask.DOKill(true);

            this.mask.color = new Color(1f, 1f, 1f, 0f);
        }
    }



    //------------ Setting Stage Data

    private void SetBackground(IReadOnlyList<Sprite> _StageSprites)
    {
        var index = (int)Define.TileSpriteName.Center;

        this.backgroundRenderer.sprite = _StageSprites[index];

        
    }

    private void SetOutlineTiles(IReadOnlyList<Sprite> _StageSprites)
    {
        var index = (int)Define.TileSpriteName.Center;

        for (int i = 0; i < outlineTiles.Length; i++)
        {
            if (i < Left_End)
            {
                index = (int)Define.TileSpriteName.Left;
                var sprite = _StageSprites[index];

                var renderer = outlineTiles[i].GetChild(0).GetComponent<SpriteRenderer>();
                renderer.sprite = sprite;
            }
            else if (i < Bottom_End)
            {
                //bottom
                index = (int)Define.TileSpriteName.Bottom;
                var sprite = _StageSprites[index];

                var renderer = outlineTiles[i].GetChild(0).GetComponent<SpriteRenderer>();
                renderer.sprite = sprite;
            }
            else if (i < Right_End)
            {
                //right
                index = (int)Define.TileSpriteName.Right;
                var sprite = _StageSprites[index];

                var renderer = outlineTiles[i].GetChild(0).GetComponent<SpriteRenderer>();
                renderer.sprite = sprite;
            }
            else
            {
                //top
                index = (int)Define.TileSpriteName.Top;
                var sprite = _StageSprites[index];

                var renderer = outlineTiles[i].GetChild(0).GetComponent<SpriteRenderer>();
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
        
        this.mask.sprite = sprite;
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