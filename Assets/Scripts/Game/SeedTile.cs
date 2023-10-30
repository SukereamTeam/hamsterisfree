using UnityEngine;
using UniRx;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;
using DataTable;
using UnityEngine.Serialization;

public class SeedTile : TileBase
{
    private bool isTouch = false;
    public bool IsTouch {
        get => this.isTouch;
        set => this.isTouch = value;
    }

    [SerializeField]
    private Define.TileType_Sub subType = Define.TileType_Sub.Default;

    private Table_Seed.Param seedData;
    
    private ITileActor tileActor;

    private bool isFuncStart = false;
    private CancellationTokenSource cts;
    private IDisposable disposable = null;




    private new void Start()
    {
        base.Start();

        this.cts = new CancellationTokenSource();
        
        GameManager.Instance.IsGame
            .Skip(TimeSpan.Zero)    // 첫 프레임 호출 스킵 (시작할 때 false 로 인해 호출되는 것 방지)
            .Where(x => x == false)
            .Subscribe(_ =>
            {
                ActClear();
            }).AddTo(this);
    }

    public override void Initialize(TileInfo _Info, Vector2 _Pos)
    {
        base.Initialize(_Info, _Pos);

        this.subType = Enum.Parse<Define.TileType_Sub>(_Info.SubType);

        var sprite = DataContainer.Instance.SeedSprites[this.info.SubType];
        if (sprite != null)
        {
            this.spriteRenderer.sprite = sprite;
        }

        this.seedData = DataContainer.Instance.SeedTable.GetParamFromType(
            this.info.SubType, this.info.SubTypeIndex);

        TileFuncStart().Forget();
    }

    private async UniTaskVoid TileFuncStart()
    {
        // 스테이지 세팅 끝나고 게임 시작할 상태가 되었을 때(IsGame == true)
        // 그 때 타일 타입마다 부여된 액션 실행
        await UniTask.WaitUntil(() => GameManager.Instance?.IsGame.Value == true);

        if (this.isFuncStart == false)
        {
            Debug.Log("SeedTile Func Start !!!");
            this.isFuncStart = true;
        }

        if (this.tileActor != null)
        {
            this.tileActor = null;
        }

        switch (this.subType)
        {
            case Define.TileType_Sub.Disappear:
            {
                this.tileActor = new TileActor_Disappear();
            }
                break;
            case Define.TileType_Sub.Moving:
            {
                this.tileActor = new TileActor_Moving();
            }
                break;
            case Define.TileType_Sub.Fade:
            {
                this.tileActor = new TileActor_Fade();
            }
                break;
        }

        if (this.tileActor != null)
        {
            var task = this.tileActor.Act(this, this.cts, this.info.ActiveTime);
            this.disposable = task.ToObservable().Subscribe(x =>
            {
                this.isFuncStart = x;
            });
        }
    }
    
    
    public override async UniTaskVoid TileTrigger()
    {
        Debug.Log($"SeedType : {this.info.SubType}, SeedValue : {this.seedData.SeedValue} 먹음");

        TileCollider.enabled = false;
        
        TriggerEvent(this.subType);

        if (this.tileActor == null)
        {
            // Default 등 Func가 따로 없는 타일들을 위한
            this.isFuncStart = false;
        }
        else
        {
            ActClear();
        }

        await UniTask.WaitUntil(() => this.isFuncStart == false);

        this.tileActor = null;

        // TODO : Delete... 테스트 용도
        this.spriteRenderer.color = Color.red;

        // TODO
        // Trigger Ani 재생
    }

    public override void Reset()
    {
        this.spriteRenderer.color = Color.white;

        ActClear();

        base.Reset();
    }

    private void TriggerEvent(Define.TileType_Sub _type)
    {
        if (_type == Define.TileType_Sub.Heart || _type == Define.TileType_Sub.Fake)
        {
            if (this.seedData.SeedValue < 0)
            {
                SoundManager.Instance.PlayOneShot(Define.SoundPath.SFX_SEED_STAGETYPE_DEC.ToString()).Forget();
            }
            else
            {
                SoundManager.Instance.PlayOneShot(Define.SoundPath.SFX_SEED_STAGETYPE_ADD.ToString()).Forget();
            }


            GameManager.Instance.StageManager.ChangeStageValue(this.seedData.SeedValue);
        }
        else
        {
            SoundManager.Instance.PlayOneShot(Define.SoundPath.SFX_SEED.ToString()).Forget();

            GameManager.Instance.SeedScore.Value++;
        }
    }

    private void ActClear()
    {
        this.cts.Cancel();
        
        if (this.disposable != null)
        {
            Debug.Log($"SeedTile Act Dispose!");
            this.disposable.Dispose();
        }

        this.cts.Dispose();
    }

    private void OnDestroy()
    {
        ActClear();
    }
}
