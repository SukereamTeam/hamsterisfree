using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class Scene_Game : Scene_Base
{
    [SerializeField]
    private Sprite testSprite;

    public override async UniTask Test()
    {
        Debug.Log("Game Scene 진입????");

        await ResLoad();//UniTask.CompletedTask;
    }

    private async UniTask ResLoad()
    {
        //Assets/Resources/Images/Map/Forest/Forest_Left.png
        var res = await Resources.LoadAsync<Sprite>("Images/Map/Forest/Forest_Left");

        testSprite = res as Sprite;
    }
}
