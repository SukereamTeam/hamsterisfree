using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DataTable;


public class DataContainer : MonoSingleton<DataContainer>
{
    [SerializeField]
    private Table_Stage stageTable;
    public Table_Stage StageTable => this.stageTable;

    [SerializeField]
    private Table_Seed seedTable;
    public Table_Seed SeedTable => this.seedTable;

    private List<Sprite> stageTileSprites;
    public List<Sprite> StageTileSprites => this.stageTileSprites;


    private const string RootPath_Stage = "Images/Map";

    private int tileSpritesCount = 0;




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
                await LoadTileSprites(item.MapName);
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

    private async UniTask LoadTileSprites(string _MapName)
    {
        this.tileSpritesCount = Enum.GetValues(typeof(Define.TileSpriteName)).Length;
        this.stageTileSprites = new List<Sprite>(this.tileSpritesCount);

        var path = $"{RootPath_Stage}/{_MapName}/{_MapName}_";

        try
        {
            for (int spriteIndex = 0; spriteIndex < this.tileSpritesCount; spriteIndex++)
            {
                var spriteName = Enum.GetName(typeof(Define.TileSpriteName), spriteIndex);

                var spritePath = $"{path}{spriteName}";

                var resource = await Resources.LoadAsync<Sprite>(spritePath);

                if (resource is Sprite sprite)
                {
                    this.stageTileSprites.Add(sprite);
                }
                else
                {
                    Debug.Log("### Fail <Sprite> Type Casting ###");
                }
            }
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            Debug.Log("### LoadTileSprites Failed: " + ex.Message + " ###");
        }
    }
}
