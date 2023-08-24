using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitTile : TileBase
{
    public override void Initialize(TileInfo _Info)
    {
        base.Initialize(_Info);

        var sprite = DataContainer.Instance.ExitSprite;
        if (sprite != null)
        {
            this.spriteRenderer.sprite = sprite;
        }
    }

    public override async UniTaskVoid TileTriggerEvent()
    {
        //Debug.Log("Game End");

        // 별 갯수 계산해서
        // 완료 처리 or 무반응

        GameManager.Instance.IsGame.Value = false;

        await UniTask.CompletedTask;
    }
}
