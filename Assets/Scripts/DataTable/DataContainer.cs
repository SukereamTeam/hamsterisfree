using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Cysharp.Threading.Tasks;
using TMPro;

public class DataContainer : MonoSingleton<DataContainer>
{
    static readonly string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
    static readonly string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
    static readonly char[] TRIM_CHARS = { '\"' };

    public static StageTable StageTable { get; private set; }
    public static SeedTable SeedTable { get; private set; }

    public static List<Sprite> StageTileSprites { get; private set; }


    private static int tileSpriteCount = 0;





    public static void Initialize()
    {
        StageTable = new StageTable();
        ReadCSV(StageTable, "StageTable");

        tileSpriteCount = Enum.GetValues(typeof(Define.TileSpriteName)).Length;
        StageTileSprites = new List<Sprite>(tileSpriteCount);
    }


    public static void ReadCSV<T>(TableBase<T> _TableBase, string _FileName)
    {
        var table = Resources.Load<TextAsset>($"Data/csv/{_FileName}");

        var lines = Regex.Split(table.text, LINE_SPLIT_RE);


        var headers = Regex.Split(lines[1], SPLIT_RE);  //csv의 둘째줄부터 header로 판정
        for (var i = 2; i < lines.Length; i++)
        {
            var values = Regex.Split(lines[i], SPLIT_RE);
            if (values.Length == 0 || values[0] == "")
            {
                continue;
            }

            for (var j = 0; j < headers.Length && j < values.Length; j++)
            {
                string value = values[j];

                value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");

                //0 / LimitTime,60 / Forest / Default,3 / 0
                _TableBase.SetTable(values[0], headers[j], value);
            }
        }
    }

    // TODO
    // Json 으로 진행상황 저장하는 함수 만들기
    // 진행해야하는 스테이지 넘버 (첫 시작이면 0이란 소리)
    // 탈출문 좌표
    // 스테이지 선택해서 게임 시작할 때 여기 저장된 스테이지 넘버로 스테이지 데이터테이블 참조하여 불러옴



    public static async UniTask LoadStageDatas()
    {
        Debug.Log("LoadStageDatas 시작");

        var curIndex = CommonManager.Instance.CurStageIndex;

        try
        {
            if (StageTable.DicData.ContainsKey(curIndex.ToString()))
            {
                var dicData = StageTable.DicData[curIndex.ToString()];


                await LoadTileSprites(dicData.MapName);
            }
            else
            {
                Debug.Log($"### Error ---> {curIndex} is Not ContainsKey ###");
            }
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            Debug.Log("### LoadTileSprites Failed: " + ex.Message + " ###");
        }

        Debug.Log("LoadStageDatas 끝!");
    }

    private static async UniTask LoadTileSprites(string _MapName)
    {
        var rootPath = "Images/Map";

        var path = $"{rootPath}/{_MapName}/{_MapName}_";

        Define.TileSpriteName spriteName = Define.TileSpriteName.Center;

        try
        {
            for (spriteName = 0; (int)spriteName < tileSpriteCount; spriteName++)
            {
                var spritePath = $"{path}{spriteName}";

                var resource = await Resources.LoadAsync<Sprite>(spritePath);
                var sprite = resource as Sprite;

                if (sprite != null)
                {
                    StageTileSprites.Add(sprite);
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
