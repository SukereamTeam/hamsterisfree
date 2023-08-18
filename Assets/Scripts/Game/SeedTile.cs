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
    [SerializeField]
    private Sprite sprite;

    [SerializeField]
    private string type;


    

    private bool isTouch = false;
    public bool IsTouch {
        get => this.isTouch;
        set => this.isTouch = value;
    }

    private void Awake()
    {

    }

    public override void Initialize(Define.TileType _Type, string _SpritePath, Vector2 _Pos, int _Root)
    {
        base.Initialize(_Type, _SpritePath, _Pos, _Root);

        this.transform.localPosition = new Vector3(_Pos.x, _Pos.y, -0.7f);

        // TODO
        // sprite 나중엔 데이터테이블 경로로 빼야함 지금은 serializeField이지만...
        // Resources.load
        // 그리고 베이스 클래스로 옮겨줘야 함 SpritePath 써서 로드하는 걸로

        //this.tileSprite = sprite;

        //this.spriteRenderer.sprite = this.tileSprite;

        this.type = _Type.ToString();
    }

    public override void TileTriggerEvent()
    {
        // Check 타일 지나감

        // TODO
        // Seed Count 증가
        this.tileCollider.enabled = false;
        // Trigger Ani 재생
        //
    }

}
