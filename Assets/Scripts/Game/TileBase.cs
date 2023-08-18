using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public abstract class TileBase : MonoBehaviour
{
    [Serializable]
    public struct TileInfo
    {
        public Define.TileType Type;
        public Vector2 Pos;
        public string SpritePath;
        public int Root;

        public string SubType;
        public float ActiveTime;
    }

    // Builder 패턴
    public class TileBuilder
    {
        private TileInfo _tileInfo = new TileInfo();

        public TileBuilder(TileInfo _Info)
        {
            _tileInfo = _Info;
        }

        /// <summary>
        /// 각 타일들의 서브 타입 지정
        /// </summary>
        /// <param name="_Type">eg. SeedTile의 Default, Disappear, Fake 타입</param>
        /// <returns></returns>
        public TileBuilder WithSubType(string _Type)
        {
            _tileInfo.SubType = _Type;
            return this;
        }

        public TileBuilder WithActiveTime(float _Time)
        {
            _tileInfo.ActiveTime = _Time;
            return this;
        }

        public TileInfo Build()
        {
            return _tileInfo;
        }
    }

    /// <summary>
    /// 각 타일이 생성되는 위치의 기반이 되는 타일의 index
    /// (ExitTile은 MapManager의 outlineTiles 중 하나일 것.. 그것의 index)
    /// (SeedTile, MonsterTile은 MapManager의 backTiles 중 하나)
    /// </summary>
    [SerializeField]
    private TileInfo info;
    public TileInfo Info => this.info;

    [SerializeField]
    protected SpriteRenderer spriteRenderer;

    protected Sprite tileSprite;

    protected BoxCollider2D tileCollider;

    protected Animator animator;

    public virtual void Initialize(TileInfo _Info)
    {
        this.transform.localPosition = new Vector3(_Info.Pos.x, _Info.Pos.y, -0.7f);

        info = _Info;

        var sprite = Resources.Load<Sprite>(_Info.SpritePath);
        if (sprite != null)
        {
            this.spriteRenderer.sprite = sprite;
        }
        else
        {
            Debug.Log($"### ERROR ---> {_Info.SpritePath} ###");
        }
    }

    public abstract void TileTriggerEvent();
}
