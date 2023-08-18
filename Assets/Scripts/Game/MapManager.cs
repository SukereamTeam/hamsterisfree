using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UniRx;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class MapManager : MonoBehaviour
{
    // 맵 생성
    [SerializeField]
    private SpriteRenderer backgroundImage;
    public SpriteRenderer Background => this.backgroundImage;

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




    private void Awake()
    {
        // 하이어라키의 타일 오브젝트들 보기 편하도록 이름 바꿔주기
        ChangeNameOutlineTiles();

        this.backTiles = this.backgroundImage.transform.parent.GetComponentsInChildren<SpriteRenderer>()
            .Select(x => x.transform)
            .Where(x => x != this.backgroundImage.transform)
            .ToArray();

        this.seedTiles = new List<SeedTile>();
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

    public void SetStage()
    {
        if (DataContainer.Instance.StageSprites.Count == 0)
        {
            Debug.Log("### Error ---> DataContainer.StageTileSprites.Count == 0 ###");
            return;
        }

        SetBackground();
        SetOutlineTiles();
        SetMask();

        var curStage = CommonManager.Instance.CurStageIndex;

        for (int i = 0; i < Enum.GetValues(typeof(Define.TileType)).Length; i++)
        {
            if (i == (int)Define.TileType.Exit)
            {
                CreateExitTile(curStage, Define.TileType.Exit);
            }
            else if (i == (int)Define.TileType.Seed)
            {
                CreateSeedTile(curStage, Define.TileType.Seed);
            }
        }

        DataContainer.Instance.StageSprites.Clear();
    }

    //------------------ Create Tiles

    private void CreateExitTile(int _CurStage, Define.TileType _TileType)
    {
        UnityEngine.Random.InitState((int)DateTime.Now.Ticks);

        

        var random = UnityEngine.Random.Range(0, outlineTiles.Length);
        var randomPos = new Vector2(outlineTiles[random].transform.localPosition.x, outlineTiles[random].transform.localPosition.y);

        var exitTile = Instantiate<GameObject>(this.exitPrefab, this.tileRoot);
        var exitScript = exitTile.GetComponent<ExitTile>();

        TileBase.TileInfo baseInfo = new TileBase.TileInfo
        {
            Type = _TileType,
            Pos = randomPos,
            SpritePath = "Images/Map/Forest/Forest_Center",
            Root = random
        };

        TileBase.TileInfo tileInfo = new TileBase.TileBuilder(baseInfo).WithSubType("").Build();

        exitScript.Initialize(tileInfo);

        // TODO
        // 하위에 탈출 셰이더(빛 효과) 메테리얼 오브젝트 추가
        // 타일 좌표에 따라 메테리얼 오브젝트 방향 바꿔줘야 함 (x > 0 ? shader 오른쪽에서 뻗어나오고 : 왼쪽에서 뻗어나오고 y > 0 ? 위에서 뻗어나오고 : 아래에서 뻗어나오고)
    }

    private void CreateSeedTile(int _CurStage, Define.TileType _TileType)
    {
        var stageTable = DataContainer.Instance.StageTable.list[_CurStage];

        UnityEngine.Random.InitState((int)DateTime.Now.Ticks);

        for (int i = 0; i < stageTable.SeedData.Count; i++)
        {
            for (int j = 0; j < stageTable.SeedData[i].Count; j++)
            {
                var random = UnityEngine.Random.Range(0, this.backTiles.Length);
                
                while (true)
                {
                    if (CheckDuplicateTile(this.seedTiles, random))
                    {
                        random = UnityEngine.Random.Range(0, this.backTiles.Length);
                    }
                    else
                    {
                        break;
                    }
                }

                var randomPos = new Vector2(this.backTiles[random].transform.position.x, this.backTiles[random].transform.position.y);

                var seedTile = Instantiate<GameObject>(seedPrefab, this.tileRoot);
                var seedScript = seedTile.GetComponent<SeedTile>();

                // eg. SeedTile 의 타입들 중 Dafault 타입에 대한 데이터를 SeedTable에서 가져오기
                var targetSeedData = DataContainer.Instance.SeedTable.GetParamFromType(stageTable.SeedData[i].Type);

                // 기본 정보 초기화
                TileBase.TileInfo baseInfo = new TileBase.TileInfo
                {
                    Type = _TileType,
                    Pos = randomPos,
                    SpritePath = targetSeedData.SpritePath,
                    Root = random
                };

                // 추가 정보 더해서 초기화 (SubType, ActiveTime)
                TileBase.TileInfo tileInfo = new TileBase.TileBuilder(baseInfo)
                    .WithSubType(stageTable.SeedData[i].Type)
                    .WithActiveTime(targetSeedData.ActiveTime)
                    .Build();

                seedScript.Initialize(tileInfo);

                this.seedTiles.Add(seedScript);
            }
        }
    }

    /// <summary>
    /// SeedTile, MonsterTile 둘 다 사용할거라 where T : TileBase 붙임
    /// </summary>
    /// <typeparam name="T">타입은 TileBase 를 상속받은 클래스여야 함</typeparam>
    /// <param name="list">체크할 Tile List</param>
    /// <param name="randomPos">중복인지 체크할 포지션</param>
    /// <returns>True를 반환하면 중복이라는 뜻</returns>
    private bool CheckDuplicateTile<T>(List<T> _List, int _RandomIndex) where T : TileBase
    {
        for (int i = 0; i < _List.Count; i++)
        {
            if (_RandomIndex.Equals(_List[i].Info.Root))
            {
                return true;
            }
        }

        return false;
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

    private void SetBackground()
    {
        var index = (int)Define.TileSpriteName.Center;

        this.backgroundImage.sprite = DataContainer.Instance.StageSprites[index];

        
    }

    private void SetOutlineTiles()
    {
        var index = (int)Define.TileSpriteName.Center;

        for (int i = 0; i < outlineTiles.Length; i++)
        {
            if (i < Left_End)
            {
                index = (int)Define.TileSpriteName.Left;
                var sprite = DataContainer.Instance.StageSprites[index];

                var renderer = outlineTiles[i].GetChild(0).GetComponent<SpriteRenderer>();
                renderer.sprite = sprite;
            }
            else if (i < Bottom_End)
            {
                //bottom
                index = (int)Define.TileSpriteName.Bottom;
                var sprite = DataContainer.Instance.StageSprites[index];

                var renderer = outlineTiles[i].GetChild(0).GetComponent<SpriteRenderer>();
                renderer.sprite = sprite;
            }
            else if (i < Right_End)
            {
                //right
                index = (int)Define.TileSpriteName.Right;
                var sprite = DataContainer.Instance.StageSprites[index];

                var renderer = outlineTiles[i].GetChild(0).GetComponent<SpriteRenderer>();
                renderer.sprite = sprite;
            }
            else
            {
                //top
                index = (int)Define.TileSpriteName.Top;
                var sprite = DataContainer.Instance.StageSprites[index];

                var renderer = outlineTiles[i].GetChild(0).GetComponent<SpriteRenderer>();
                renderer.sprite = sprite;
            }
        }


        for (index = (int)Define.TileSpriteName.TopLeft; index <= (int)Define.TileSpriteName.BottomRight; index++)
        {
            var tileIndex = index - (int)Define.TileSpriteName.TopLeft;
            this.edgeTiles[tileIndex].sprite = DataContainer.Instance.StageSprites[index];
        }
    }


    private void SetMask()
    {
        Debug.Log("SetMask 시작");
        var sprite = DataContainer.Instance.StageSprites[(int)Define.TileSpriteName.Mask];
        
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