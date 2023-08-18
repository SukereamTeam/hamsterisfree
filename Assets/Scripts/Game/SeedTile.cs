using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum SeedType
{
    Normal = 0,
    Heart
}


public class SeedTile : TileBase
{

    private bool isTouch = false;
    public bool IsTouch {
        get => this.isTouch;
        set => this.isTouch = value;
    }




    public override void Initialize(TileInfo _Info)
    {
        base.Initialize(_Info);

        var sprite = DataContainer.Instance.SeedSprites[this.info.SubType];

        if (sprite != null)
        {
            this.spriteRenderer.sprite = sprite;
        }
    }

    public override void TileTriggerEvent()
    {
        // Check 타일 지나감

        Debug.Log("Seed 먹음");

        this.tileCollider.enabled = false;

        // TODO : Delete... 테스트 용도
        this.spriteRenderer.color = Color.red;

        GameManager.Instance.SeedCount++;

        // TODO
        // Trigger Ani 재생
        
    }

}
