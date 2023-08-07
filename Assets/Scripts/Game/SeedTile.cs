using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SeedTile : TileBase
{
    [SerializeField]
    private Sprite sprite = null;

    private bool isTouch = false;
    public bool IsTouch {
        get => this.isTouch;
        set => this.isTouch = value;
    }

    private void Awake()
    {

    }

    public override void Initialize(Define.TileType _Type, string _SpritePath, Vector2 _Pos)
    {
        base.Initialize(_Type, _SpritePath, _Pos);

        // TODO
        // sprite 나중엔 데이터테이블 경로로 빼야함 지금은 serializeField이지만...
        // Resources.load
        // 그리고 베이스 클래스로 옮겨줘야 함 SpritePath 써서 로드하는 걸로
        this.tileSprite = sprite;
        this.spriteRenderer.sprite = this.tileSprite;

        
    }

    public override void TileTriggerEvent()
    {
        // Check 타일 지나감
    }

}
