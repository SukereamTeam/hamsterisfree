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
    private SpriteRenderer background = null;
    public SpriteRenderer Background => this.background;

    [SerializeField]
    private Transform[] backTiles = null;

    [SerializeField]
    private Transform[] outlineTiles = null;

    [SerializeField]
    private SpriteRenderer[] edgeTiles = null;

    [SerializeField]
    private SpriteRenderer mask;
    public SpriteRenderer Mask => this.mask;

    [SerializeField]
    private float fadeTime = 0f;
    public float FadeTime => this.fadeTime;

    [SerializeField]
    private GameObject exitPrefab = null;

    [SerializeField]
    private Image fadeImage = null;


    private const int Left_End = 9;
    private const int Bottom_End = 15;
    private const int Right_End = 24;


    private IReactiveProperty<bool> isFade = new ReactiveProperty<bool>(false);
    public IReactiveProperty<bool> IsFade
    {
        get => this.isFade;
        set => this.isFade = value;
    }





    private void Awake()
    {
        ChangeNameOutlineTiles();
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
        if (DataContainer.Instance.StageTileSprites.Count == 0)
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
                
                CreateExitTile();
            }
            else if (i == (int)Define.TileType.Seed)
            {
                CreateSeedTile();
            }
        }

        DataContainer.Instance.StageTileSprites.Clear();
    }

    private void CreateSeedTile()
    {
        var stageTable = DataContainer.Instance.StageTable.list[CommonManager.Instance.CurStageIndex];
        
        for (int i = 0; i < stageTable.SeedData.Count; i++)
        {
            for (int j = 0; j < stageTable.SeedData[i].Count; j++)
            {
                //instantiate
            }
        }
    }

    private void ChangeNameOutlineTiles()
    {
        for (int i = 0; i < outlineTiles.Length; i++)
        {
            outlineTiles[i].name = $"Tile ({outlineTiles[i].localPosition.x}, {outlineTiles[i].localPosition.y})";
        }
    }

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

        this.background.sprite = DataContainer.Instance.StageTileSprites[index];

        
    }

    private void SetOutlineTiles()
    {
        var index = (int)Define.TileSpriteName.Center;

        for (int i = 0; i < outlineTiles.Length; i++)
        {
            if (i < Left_End)
            {
                index = (int)Define.TileSpriteName.Left;
                var sprite = DataContainer.Instance.StageTileSprites[index];

                var renderer = outlineTiles[i].GetChild(0).GetComponent<SpriteRenderer>();
                renderer.sprite = sprite;
            }
            else if (i < Bottom_End)
            {
                //bottom
                index = (int)Define.TileSpriteName.Bottom;
                var sprite = DataContainer.Instance.StageTileSprites[index];

                var renderer = outlineTiles[i].GetChild(0).GetComponent<SpriteRenderer>();
                renderer.sprite = sprite;
            }
            else if (i < Right_End)
            {
                //right
                index = (int)Define.TileSpriteName.Right;
                var sprite = DataContainer.Instance.StageTileSprites[index];

                var renderer = outlineTiles[i].GetChild(0).GetComponent<SpriteRenderer>();
                renderer.sprite = sprite;
            }
            else
            {
                //top
                index = (int)Define.TileSpriteName.Top;
                var sprite = DataContainer.Instance.StageTileSprites[index];

                var renderer = outlineTiles[i].GetChild(0).GetComponent<SpriteRenderer>();
                renderer.sprite = sprite;
            }
        }


        for (index = (int)Define.TileSpriteName.TopLeft; index <= (int)Define.TileSpriteName.BottomRight; index++)
        {
            var tileIndex = index - (int)Define.TileSpriteName.TopLeft;
            this.edgeTiles[tileIndex].sprite = DataContainer.Instance.StageTileSprites[index];
        }
    }


    private void SetMask()
    {
        Debug.Log("SetMask 시작");
        var sprite = DataContainer.Instance.StageTileSprites[(int)Define.TileSpriteName.Mask];
        
        this.mask.sprite = sprite;
        Debug.Log("SetMask 끝");
    }


    private void CreateExitTile()
    {
        UnityEngine.Random.InitState((int)DateTime.Now.Ticks);

        var random = UnityEngine.Random.Range(0, outlineTiles.Length);
        var randomPos = new Vector2(outlineTiles[random].transform.localPosition.x, outlineTiles[random].transform.localPosition.y);

        var exitTile = Instantiate<GameObject>(exitPrefab, this.transform);
        var exitScript = exitTile.GetComponent<ExitTile>();
        exitScript.Initialize(Define.TileType.Exit, "", randomPos);

        // TODO
        // 하위에 탈출 셰이더(빛 효과) 메테리얼 오브젝트 추가
        // 타일 좌표에 따라 메테리얼 오브젝트 방향 바꿔줘야 함 (x > 0 ? shader 오른쪽에서 뻗어나오고 : 왼쪽에서 뻗어나오고 y > 0 ? 위에서 뻗어나오고 : 아래에서 뻗어나오고)
    }

    

    private void CreateSeedTile(int _CurStage)
    {
        var table = DataContainer.Instance.StageTable.list[_CurStage];

        foreach(var seed in table.SeedData)
        {
            
        }
    }
}