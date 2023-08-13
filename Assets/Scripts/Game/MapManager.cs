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

    public async UniTask SetStage()
    {
        if (DataContainer.StageTileSprites.Count == 0)
        {
            Debug.Log("### Error ---> DataContainer.StageTileSprites.Count == 0 ###");
            return;
        }

        SetBackground();
        SetOutlineTiles();
        SetMask();

        CreateExitTile();

        DataContainer.StageTileSprites.Clear();

        await UniTask.CompletedTask;
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

        this.background.sprite = DataContainer.StageTileSprites[index];

        
    }

    private void SetOutlineTiles()
    {
        var index = (int)Define.TileSpriteName.Center;

        int LeftEnd = 9;
        int BottomEnd = 15;
        int RightEnd = 24;


        for (int i = 0; i < outlineTiles.Length; i++)
        {
            if (i < LeftEnd)
            {
                index = (int)Define.TileSpriteName.Left;
                var sprite = DataContainer.StageTileSprites[index];

                var renderer = outlineTiles[i].GetChild(0).GetComponent<SpriteRenderer>();
                renderer.sprite = sprite;
            }
            else if (i < BottomEnd)
            {
                //bottom
                index = (int)Define.TileSpriteName.Bottom;
                var sprite = DataContainer.StageTileSprites[index];

                var renderer = outlineTiles[i].GetChild(0).GetComponent<SpriteRenderer>();
                renderer.sprite = sprite;
            }
            else if (i < RightEnd)
            {
                //right
                index = (int)Define.TileSpriteName.Right;
                var sprite = DataContainer.StageTileSprites[index];

                var renderer = outlineTiles[i].GetChild(0).GetComponent<SpriteRenderer>();
                renderer.sprite = sprite;
            }
            else
            {
                //top
                index = (int)Define.TileSpriteName.Top;
                var sprite = DataContainer.StageTileSprites[index];

                var renderer = outlineTiles[i].GetChild(0).GetComponent<SpriteRenderer>();
                renderer.sprite = sprite;
            }
        }


        for (index = (int)Define.TileSpriteName.TopLeft; index <= (int)Define.TileSpriteName.BottomRight; index++)
        {
            var tileIndex = index - (int)Define.TileSpriteName.TopLeft;
            this.edgeTiles[tileIndex].sprite = DataContainer.StageTileSprites[index];
        }
    }


    private void SetMask()
    {
        Debug.Log("SetMask 시작");
        var sprite = DataContainer.StageTileSprites[(int)Define.TileSpriteName.Mask];
        
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

    

    private void CreateTile()
    {

    }
}