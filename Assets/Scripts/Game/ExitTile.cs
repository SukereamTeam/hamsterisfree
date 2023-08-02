using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitTile : TileBase
{
    [SerializeField]
    private Sprite sprite;

    public override void Initialize(TileType _Type, string _SpritePath, Vector2 _Pos)
    {
        base.Initialize(_Type, _SpritePath, _Pos);

        // TODO
        // sprite 나중엔 데이터테이블 경로로 빼야함 지금은 serializeField이지만...
        // Resources.load
        // 그리고 베이스 클래스로 옮겨줘야 함 SpritePath 써서 로드하는 걸로
        //this.tileSprite = sprite;
        //this.spriteRenderer.sprite = this.tileSprite;


    }

    public override void TileTriggerEvent()
    {
        //Debug.Log("Game End");

        // 별 갯수 계산해서
        // 완료 처리 or 무반응
    }
}
