using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

public class Scene_Game : Scene_Base
{
    private List<Sprite> tileSpriteList;
    public List<Sprite> TileSpriteList => this.tileSpriteList;


    private readonly string RootMapPath = "Images/Map";
    private Define.TileSpriteName spriteName;


    public override async UniTask LoadDatas()
    {
        Debug.Log("### Scene_Game LoadDatas ###");

        var curStageIndex = CommonManager.Instance.CurStageIndex.ToString();
        var stageData = DataContainer.StageTable.DicData[curStageIndex];

        var path = $"{RootMapPath}/{stageData.MapName}/{stageData.MapName}_";

        await LoadSpriteList(path);

        await GameManager.Instance.GameStart();
    }

    

    private async UniTask LoadSpriteList(string rootPath)
    {
        var tileNameCount = Enum.GetValues(typeof(Define.TileSpriteName)).Length;
        this.tileSpriteList = new List<Sprite>(tileNameCount);

        try
        {
            var path = rootPath;

            for (spriteName = 0; (int)spriteName < tileNameCount; spriteName++)
            {
                path = $"{rootPath}{spriteName}";

                var resource = await Resources.LoadAsync<Sprite>(path);
                var sprite = resource as Sprite;

                if (sprite != null)
                {
                    this.tileSpriteList.Add(sprite);
                }
                else
                {
                    Debug.Log("### Fail <Sprite> Type Casting ###");
                }
            }


            
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            Debug.Log("### Resource Load Failed: " + ex.Message + " ###");
        }
    }
}
