using UnityEngine;
using UniRx;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;
using DataTable;
using DG.Tweening;

public class SeedTile : TileBase
{
    private bool isTouch = false;
    public bool IsTouch {
        get => isTouch;
        set => isTouch = value;
    }

    [SerializeField]
    private Define.TileType_Sub subType = Define.TileType_Sub.Default;

    private Table_Seed.Param seedData;
    
    private ITileActor tileActor;
    private CancellationTokenSource cts;
    private IDisposable actDisposable = null;

    private const float TweenTime = 0.25f;

    public bool IsFuncStart { get; private set; }



    private new void Start()
    {
        base.Start();

        cts = new CancellationTokenSource();
        IsFuncStart = false;


        GameManager.Instance.IsGame
            .Skip(TimeSpan.Zero)    // 첫 프레임 호출 스킵 (시작할 때 false 로 인해 호출되는 것 방지)
            .Where(x => x == false)
            .Subscribe(_ =>
            {
                ActClear();
            }).AddTo(this);
    }

    public override void Initialize(TileInfo info, Vector2 pos)
    {
        base.Initialize(info, pos);

        subType = Enum.Parse<Define.TileType_Sub>(info.SubType);

        var sprite = DataContainer.Instance.SeedSprites[base._tileInfo.SubType];
        if (sprite != null)
        {
            spriteRenderer.sprite = sprite;
        }

        seedData = DataContainer.Instance.SeedTable.GetParamFromType(
            base._tileInfo.SubType, base._tileInfo.SubTypeIndex);
    }

    public void TileFuncStart()
    {
        // 스테이지 세팅 끝나고 게임 시작할 상태가 되었을 때(IsGame == true)
        // 그 때 타일 타입마다 부여된 액션 실행
        
        if (IsFuncStart == true)
            return;
        
        IsFuncStart = true;
        tileActor = null;
        
        if (cts == null || cts.IsCancellationRequested == true)
            cts = new CancellationTokenSource();

        switch (subType)
        {
            case Define.TileType_Sub.Disappear:
            {
                tileActor = new TileActor_Disappear();
            }break;
            case Define.TileType_Sub.Moving:
            {
                tileActor = new TileActor_Moving();
            }break;
            case Define.TileType_Sub.Fade:
            {
                tileActor = new TileActor_Fade();
            }break;
        }

        if (tileActor != null)
        {
            var task = tileActor.Act(this, cts, _tileInfo.ActiveTime);
            actDisposable = task.ToObservable().Subscribe(x =>
            {
                IsFuncStart = x;
            });
        }
    }
    
    
    public override async UniTaskVoid TileTrigger()
    {
        Debug.Log($"SeedType : {_tileInfo.SubType}, SeedValue : {seedData.SeedValue} 먹음");

        TileCollider.enabled = false;
        
        TriggerEvent(subType);

        if (tileActor == null)
        {
            // Default 등 Func가 따로 없는 타일들을 위한
            IsFuncStart = false;
        }
        else
        {
            ActClear();
            tileActor = null;
        }

        await UniTask.WaitUntil(() => IsFuncStart == false/*, cancellationToken: cts.Token*/);

        await transform.DOScale(0f, TweenTime).SetEase(Ease.InOutBack);
    }

    public override void Reset()
    {
        transform.DOScale(1f, TweenTime).SetEase(Ease.Linear);
        TileCollider.enabled = true;

        ActClear();

        base.Reset();
    }

    private void TriggerEvent(Define.TileType_Sub type)
    {
        if (type == Define.TileType_Sub.Heart || type == Define.TileType_Sub.Fake)
        {
            if (seedData.SeedValue < 0)
            {
                SoundManager.Instance.PlayOneShot(Define.SoundPath.SFX_SEED_STAGETYPE_DEC.ToString()).Forget();
            }
            else
            {
                SoundManager.Instance.PlayOneShot(Define.SoundPath.SFX_SEED_STAGETYPE_ADD.ToString()).Forget();
            }


            GameManager.Instance.StageManager.ChangeStageValue(seedData.SeedValue);
        }
        else
        {
            SoundManager.Instance.PlayOneShot(Define.SoundPath.SFX_SEED.ToString()).Forget();

            GameManager.Instance.SeedScore.Value++;
        }
    }

    private void ActClear()
    {
        if (IsFuncStart == true)
        {
            IsFuncStart = false;
        }

        if (cts != null || cts?.IsCancellationRequested == false)
        {
            cts.Cancel();
        }

        transform?.DOKill(true);
        
        actDisposable?.Dispose();
    }

    private void OnDestroy()
    {
        ActClear();

        cts?.Dispose();
    }
}
