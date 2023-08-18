using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitTile : TileBase
{
    [SerializeField]
    private Sprite sprite;

    public override void Initialize(TileInfo _Info)
    {
        base.Initialize(_Info);
    }

    public override void TileTriggerEvent()
    {
        //Debug.Log("Game End");

        // 별 갯수 계산해서
        // 완료 처리 or 무반응

        GameManager.Instance.IsGame.Value = false;

        
    }
}
