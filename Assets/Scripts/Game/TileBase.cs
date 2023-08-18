using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public abstract class TileBase : MonoBehaviour
{
    public struct TileInfo
    {
        public Define.TileType Type;
        public string SubType;
        public Vector2 Pos;
        public string SpritePath;
        public int Root;
    }

    /// <summary>
    /// 각 타일이 생성되는 위치의 기반이 되는 타일이 어떤건지.
    /// (ExitTile은 MapManager의 outlineTiles 중 하나일 것)
    /// (SeedTile, MonsterTile은 MapManager의 backTiles 중 하나)
    /// </summary>
    protected int rootTile;         
    public int RootTile => this.rootTile;


    protected Define.TileType tileType;
    public Define.TileType TileType => this.tileType;

    protected Sprite tileSprite;
    protected SpriteRenderer spriteRenderer;
    protected BoxCollider2D tileCollider;
    protected Animator animator;

    public virtual void Initialize(TileInfo _Info)
    {
        this.tileType = _Info.Type;

        this.transform.localPosition = new Vector3(_Info.Pos.x, _Info.Pos.y, -0.7f);

        this.rootTile = _Info.Root;
    }

    public abstract void TileTriggerEvent();
}
