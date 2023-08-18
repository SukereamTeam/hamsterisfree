using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DataTable;
using System.Threading.Tasks;

public class DataContainer : MonoSingleton<DataContainer>
{
    [SerializeField]
    private Table_Stage stageTable;
    public Table_Stage StageTable => this.stageTable;

    [SerializeField]
    private Table_Seed seedTable;
    public Table_Seed SeedTable => this.seedTable;



    private const string RootPath_Stage = "Images/Map";

    private List<Sprite> stageSprites;
    public List<Sprite> StageSprites => this.stageSprites;

    private int tileSpritesCount = 0;

    private Sprite exitSprite;
    public Sprite ExitSprite => this.exitSprite;

    private Dictionary<string, Sprite> seedSprites;
    public Dictionary<string, Sprite> SeedSprites => this.seedSprites;





    public void Initialize()
    {
        
    }


    

    // TODO
    // Json 으로 진행상황 저장하는 함수 만들기
    // 진행해야하는 스테이지 넘버 (첫 시작이면 0이란 소리)
    // 탈출문 좌표
    // 스테이지 선택해서 게임 시작할 때 여기 저장된 스테이지 넘버로 스테이지 데이터테이블 참조하여 불러옴



    public async UniTask LoadStageDatas(int _StageIndex)
    {
        Debug.Log("LoadStageDatas 시작");

        try
        {
            var item = stageTable.list.Where(x => x.Index == _StageIndex).FirstOrDefault();

            if (item != null)
            {
                await LoadStageSprites(item.MapName);

                // TODO : Modify
                this.exitSprite = await Resources.LoadAsync<Sprite>("Images/Map/Forest/Forest_Center") as Sprite;
                if (this.exitSprite == null)
                    Debug.Log("### ERROR ---> ExitSprite is Null ###");

                await LoadSeedSprites(item);
            }
            else
            {
                Debug.Log($"### Error ---> {_StageIndex} is Not ContainsKey ###");
            }
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            Debug.Log("### LoadTileSprites Failed: " + ex.Message + " ###");
        }

        Debug.Log("LoadStageDatas 끝!");
    }


    private async UniTask LoadStageSprites(string _MapName)
    {
        this.tileSpritesCount = Enum.GetValues(typeof(Define.TileSpriteName)).Length;
        this.stageSprites = new List<Sprite>(this.tileSpritesCount);

        var path = $"{RootPath_Stage}/{_MapName}/{_MapName}_";

        try
        {
            for (Define.TileSpriteName spriteName = 0; (int)spriteName < this.tileSpritesCount; spriteName++)
            {
                var spritePath = $"{path}{spriteName}";

                var resource = await Resources.LoadAsync<Sprite>(spritePath);

                if (resource is Sprite sprite)
                {
                    this.stageSprites.Add(sprite);
                }
                else
                    Debug.Log("### Fail <Sprite> Type Casting ###");
            }
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            Debug.Log("### LoadTileSprites Failed: " + ex.Message + " ###");
        }
    }

    private async UniTask LoadSeedSprites(Table_Stage.Param item)
    {
        var seedCount = item.SeedData.Count;

        this.seedSprites = new Dictionary<string, Sprite>(seedCount);

        for (int i = 0; i < seedCount; i++)
        {
            var seedData = this.seedTable.GetParamFromType(item.SeedData[i].Type);

            var sprite = await Resources.LoadAsync<Sprite>(seedData.SpritePath) as Sprite;

            if (sprite != null)
            {
                this.seedSprites.Add(item.SeedData[i].Type, sprite);
            }
            else
                Debug.Log($"### ERROR LoadSeedSprites ---> {seedData.Type} ###");
        }
    }
}
