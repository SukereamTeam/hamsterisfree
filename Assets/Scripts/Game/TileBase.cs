using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum TileType
{
    Normal = 0,
    Exit,
    Seed,
    Monster
}

public enum TilePattern
{
    Random = 0,
    Vertical,
    Horizontal,
    Corner,
    Snake,
    Border
}


public abstract class TileBase : MonoBehaviour
{
    protected string tileName;
    protected Vector2 position;

    protected TileType tileType;
    public TileType TileType => this.tileType;

    protected Sprite tileSprite;
    protected SpriteRenderer spriteRenderer;
    protected BoxCollider2D tileCollider;
    protected Animator animator;

    public virtual void Initialize(TileType _Type, string _SpritePath, Vector2 _Pos)
    {
        this.tileType = _Type;

        this.position = _Pos;
        this.transform.localPosition = this.position;
    }

    public abstract void TileTriggerEvent();
}
