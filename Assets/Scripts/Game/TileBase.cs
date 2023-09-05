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
        public string Func;
        public int SeedValue;
    }

    // Builder 패턴
    public class TileBuilder
    {
        private TileInfo _tileInfo = new TileInfo();

        public TileBuilder(TileInfo _Info)
        {
            _tileInfo = _Info;

            // 추가 정보 기본값으로 초기화
            _tileInfo.SubType = "";
            _tileInfo.ActiveTime = 0f;
            _tileInfo.SeedValue = 0;
            _tileInfo.Func = "";
        }

        /// <summary>
        /// 각 타일들의 서브 타입 지정
        /// </summary>
        /// <param name="_Type">eg. SeedTile의 Default, Disappear, Fake 타입</param>
        public TileBuilder WithSubType(string _Type)
        {
            _tileInfo.SubType = _Type;
            return this;
        }
        
        /// <summary>
        /// 서브 타입의 Index 지정
        /// </summary>
        /// <param name="_Index">Fade0, Fade1 이런식임</param>
        /// <returns></returns>
        public TileBuilder WithSubTypeIndex(int _Index)
        {
            _tileInfo.SubTypeIndex = _Index;
            return this;
        }


        /// <summary>
        /// 각 타일들이 맵에서 보여지는 시간 지정 
        /// </summary>
        /// <param name="_Time">eg. 1 : 쭉, 0.5 : 0.5초 간격으로 보였다가 사라지기 등..</param>
        public TileBuilder WithActiveTime(float _Time)
        {
            _tileInfo.ActiveTime = _Time;
            return this;
        }

        public TileBuilder WithSeedValue(int _Value)
        {
            _tileInfo.SeedValue = _Value;
            return this;
        }

        /// <summary>
        /// MonsterTile에 부여되는 Func 세팅 (For Boss)
        /// </summary>
        /// <param name="_Func"></param>
        /// <returns></returns>
        public TileBuilder WithFunc(string _Func)
        {
            _tileInfo.Func = _Func;
            return this;
        }

        public TileInfo Build()
        {
            return _tileInfo;
        }
    }


    [SerializeField]
    protected TileInfo info;
    public TileInfo Info => this.info;

    [SerializeField]
    protected SpriteRenderer spriteRenderer;
    public SpriteRenderer SpriteRenderer => this.spriteRenderer;

    protected Sprite tileSprite;

    protected BoxCollider2D tileCollider;
    public BoxCollider2D TileCollider => this.tileCollider;

    protected Animator animator;
    

    public const float TILE_FADE_TIME = 0.3f;



    protected void Start()
    {
        this.spriteRenderer = this.GetComponent<SpriteRenderer>();

        this.tileCollider = this.GetComponent<BoxCollider2D>();

    }

    public virtual void Initialize(TileInfo _Info, Vector2 _Pos)
    {
        this.transform.localPosition = new Vector3(_Pos.x, _Pos.y, 0f);

        info = _Info;
    }

    // For tiles with a SubType of Moving (Common SeedTile, MonsterTile)
    public void SetPosition(int _RootIdx, Vector2 _Pos)
    {
        this.info.RootIdx = _RootIdx;

        this.transform.localPosition = new Vector3(_Pos.x, _Pos.y, 0f);
    }


    public abstract UniTaskVoid TileTrigger();
}
