using UnityEngine;
using UniRx;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.Serialization;

public class SeedTile : TileBase
{
    private bool isTouch = false;
    public bool IsTouch {
        get => this.isTouch;
        set => this.isTouch = value;
    }

    [FormerlySerializedAs("tileType")] [SerializeField]
    private Define.SeedTile_Type seedType = Define.SeedTile_Type.Default;

    
    private ITileActor tileActor;

    private bool isFuncStart = false;
    private CancellationTokenSource cts;
    private IDisposable disposable = null;




    private new void Start()
    {
        base.Start();

        this.cts = new CancellationTokenSource();
    }

    public override void Initialize(TileInfo _Info, Vector2 _Pos)
    {
        base.Initialize(_Info, _Pos);

        this.seedType = (Define.SeedTile_Type)Enum.Parse(typeof(Define.SeedTile_Type), _Info.SubType);

        var sprite = DataContainer.Instance.SeedSprites[this.info.SubType];
        if (sprite != null)
        {
            this.spriteRenderer.sprite = sprite;
        }

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

        switch (this.seedType)
        {
            case Define.SeedTile_Type.Disappear:
            {
                this.tileActor = new TileActor_Disappear();
            }
                break;
            case Define.SeedTile_Type.Moving:
            {
                this.tileActor = new TileActor_Moving();
            }
                break;
            case Define.SeedTile_Type.Fade:
            {
                this.tileActor = new TileActor_Fade();
            }
                break;
        }

        if (this.tileActor != null)
        {
            var task = this.tileActor.Act(this, this.info.ActiveTime, this.cts);
            this.disposable = task.ToObservable().Subscribe(x =>
            {
                this.isFuncStart = x;
            });
        }
    }
    
    
    public override async UniTaskVoid TileTrigger()
    {
        Debug.Log($"SeedType : {this.info.SubType}, SeedValue : {this.info.SeedValue} 먹음");

        this.tileCollider.enabled = false;
        
        TriggerEvent(this.seedType);

        if (this.tileActor == null)
        {
            // Default 등 Func가 따로 없는 타일들을 위한
            this.isFuncStart = false;
        }
        else
        {
            this.cts.Cancel();
        }

        await UniTask.WaitUntil(() => this.isFuncStart == false);

        this.tileActor = null;

        // TODO : Delete... 테스트 용도
        this.spriteRenderer.color = Color.red;

        // TODO
        // Trigger Ani 재생
    }

    private void TriggerEvent(Define.SeedTile_Type _type)
    {
        if (_type == Define.SeedTile_Type.Heart || _type == Define.SeedTile_Type.Fake)
        {
            GameManager.Instance.StageManager.ChangeStageValue(this.info.SeedValue);
        }
        else
        {
            GameManager.Instance.SeedScore.Value++;
        }
    }

    private void OnDestroy()
    {
        this.cts.Cancel();
        
        if (this.disposable != null)
        {
            Debug.Log("Dispose!!!");
            this.disposable.Dispose();
        }
    }
}
