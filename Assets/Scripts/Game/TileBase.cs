using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public abstract class TileBase : MonoBehaviour
{

    protected string tileName;
    protected Vector2 position;

    [SerializeField]
    protected int rootTile;         // 각 타일이 생성되는 위치의 기반이 되는 타일이 어떤건지.
                                    // (ExitTile은 MapManager의 outlineTiles 중 하나일 것)
                                    // (SeedTile, MonsterTile은 MapManager의 backTiles 중 하나)
    public int RootTile => this.rootTile;


    protected Define.TileType tileType;
    public Define.TileType TileType => this.tileType;

    protected Sprite tileSprite;
    protected SpriteRenderer spriteRenderer;
    protected BoxCollider2D tileCollider;
    protected Animator animator;

    public virtual void Initialize(Define.TileType _Type, string _SpritePath, Vector2 _Pos, int _Root)
    {
        this.tileType = _Type;

        this.position = _Pos;
        this.transform.localPosition = this.position;

        this.rootTile = _Root;
    }

    public abstract void TileTriggerEvent();
}
