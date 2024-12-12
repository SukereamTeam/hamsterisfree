using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

public abstract class TileBase : MonoBehaviour
{
    [Serializable]
    public struct TileInfo
    {
        public Define.TileType Type;
        public int RootIdx;

        public string SubType;
        public int SubTypeIndex;
        public float ActiveTime;
    }

    // Builder 패턴
    public class TileBuilder
    {
        private TileInfo _tileInfo = new TileInfo();

        public TileBuilder(TileInfo info)
        {
            _tileInfo = info;

            // 추가 정보 기본값으로 초기화
            _tileInfo.SubType = "";
            _tileInfo.SubTypeIndex = -1;
            _tileInfo.ActiveTime = 0f;
        }

        /// <summary>
        /// 각 타일들의 서브 타입 지정
        /// </summary>
        /// <param name="type">eg. SeedTile의 Default, Disappear, Fake 타입</param>
        public TileBuilder WithSubType(string type)
        {
            _tileInfo.SubType = type;
            return this;
        }
        
        /// <summary>
        /// 서브 타입의 Index 지정
        /// </summary>
        /// <param name="index">Fade0, Fade1 이런식임</param>
        /// <returns></returns>
        public TileBuilder WithSubTypeIndex(int index)
        {
            _tileInfo.SubTypeIndex = index;
            return this;
        }


        /// <summary>
        /// 각 타일들이 맵에서 보여지는 시간 지정 
        /// </summary>
        /// <param name="time">eg. 1 : 쭉, 0.5 : 0.5초 간격으로 보였다가 사라지기 등..</param>
        public TileBuilder WithActiveTime(float time)
        {
            _tileInfo.ActiveTime = time;
            return this;
        }

        public TileInfo Build()
        {
            return _tileInfo;
        }
    }


    [SerializeField]
    protected TileInfo _tileInfo;
    public TileInfo Info => _tileInfo;

    [SerializeField]
    protected SpriteRenderer spriteRenderer;
    public SpriteRenderer SpriteRenderer => spriteRenderer;


    protected Sprite tileSprite;
    protected Animator animator;
    protected Vector3 initPos = Vector3.zero;

    public CircleCollider2D TileCollider { get; protected set; }
    
    public const float TILE_FADE_TIME = 0.3f;
    
    protected void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        TileCollider = GetComponent<CircleCollider2D>();
    }

    public virtual void Initialize(TileInfo info, Vector2 pos)
    {
        initPos = new Vector3(pos.x, pos.y, 0f);
        transform.localPosition = initPos;

        _tileInfo = info;
    }

    // For tiles with a SubType of Moving (Common SeedTile, MonsterTile)
    public void SetPosition(int rootIdx, Vector2 pos)
    {
	    _tileInfo.RootIdx = rootIdx;

        transform.localPosition = new Vector3(pos.x, pos.y, 0f);
    }

    public virtual void Reset()
    {
        transform.localPosition = initPos;
    }


    public abstract UniTaskVoid TileTrigger();
}
