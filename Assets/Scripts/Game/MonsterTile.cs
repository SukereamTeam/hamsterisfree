using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;
using Cysharp.Threading.Tasks;
using DataTable;
using UnityEngine;
using UniRx;

public class MonsterTile : TileBase
{
    [SerializeField]
    private Define.TileType_Sub subType = Define.TileType_Sub.Default;
    
    private ITileActor tileActor;
    private bool isMovingDone = false;

    private Vector2 originPos = new Vector2();
    private Vector2 startPos = new Vector2();
    private Vector2 endPos = new Vector2();
    
    private Table_Monster.Param monsterData;
    private Define.TileType_Sub bossFunc = Define.TileType_Sub.Default;
    
    private CancellationTokenSource moveCts;
    private CancellationTokenSource actCts;
    private IDisposable actDisposable = null;

    public bool IsFuncStart { get; private set; }


    private new void Start()
    {
        base.Start();

        IsFuncStart = false;

        this.actCts = new CancellationTokenSource();
        this.moveCts = new CancellationTokenSource();
        
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
        var posTuple = GetStartEndPosition(_Pos);
        this.startPos = posTuple._Start;
        this.endPos = posTuple._End;

        this.originPos = _Pos;
        
        base.Initialize(_Info, this.startPos);
        
        this.subType = Enum.Parse<Define.TileType_Sub>(_Info.SubType);

        var sprite = DataContainer.Instance.MonsterSprites[this.info.SubType];
        if (sprite != null)
        {
            this.spriteRenderer.sprite = sprite;
            this.spriteRenderer.color = Color.cyan;
        }
        
        this.monsterData = DataContainer.Instance.MonsterTable.GetParamFromType(
            this.info.SubType, this.info.SubTypeIndex);

        if (this.monsterData.Func != string.Empty)
        {
            this.bossFunc = Enum.Parse<Define.TileType_Sub>(this.monsterData.Func);
        }
        
        
        //TileFuncStart().Forget();
    }

    public override void Reset()
    {


        this.spriteRenderer.color = Color.white;
        TileCollider.enabled = true;

        ActClear();

        base.Reset();
    }

    public void TileFuncStart()
    {
        // 스테이지 세팅 끝나고 게임 시작할 상태가 되었을 때(IsGame == true)
        // 그 때 타일 타입마다 부여된 액션 실행
        //await UniTask.WaitUntil(() => GameManager.Instance?.IsGame.Value == true);

        if (IsFuncStart == true)
        {
            return;
        }
        else if (IsFuncStart == false)
        {
            Debug.Log("Monster Func Start !!!");
            IsFuncStart = true;
        }

        if (this.actCts == null || this.actCts.IsCancellationRequested == true)
        {
            this.actCts = new CancellationTokenSource();
        }

        if (this.moveCts == null || this.moveCts.IsCancellationRequested == true)
        {
            this.moveCts = new CancellationTokenSource();
        }

        Move(this.info.ActiveTime, this.monsterData.MoveSpeed, this.moveCts).Forget();
        
        this.tileActor = null;
        
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
            case Define.TileType_Sub.Boss:
            {
                if (bossFunc == Define.TileType_Sub.Moving)
                {
                    this.tileActor = new TileActor_Moving();
                }
            }
                break;
        }
        
        if (this.tileActor != null)
        {
            if (this.subType == Define.TileType_Sub.Moving || this.bossFunc == Define.TileType_Sub.Moving)
            {
                return;
            }
            
            var task = this.tileActor.Act(this, this.actCts, this.info.ActiveTime);
            this.actDisposable = task.ToObservable().Subscribe(x =>
            {
                IsFuncStart = x;
            });
        }
        
    }

    private async UniTaskVoid Move(float _Time, float _Speed, CancellationTokenSource _Cts)
    {
        float timer = 0f;
        float progress = 0f;

        try
        {
            while (_Cts.IsCancellationRequested == false)
            {
                timer += Time.deltaTime;
            
                progress += Time.deltaTime * _Speed; //elapsedTime / _Time;
                progress = Mathf.Clamp01(progress);       // 0~1 사이 값 유지
            
                var newPosition = Vector3.Lerp(this.startPos, this.endPos, progress);
            
                this.transform.localPosition = new Vector3(newPosition.x, newPosition.y, -0.5f);
            
                if (progress >= 1f)
                {
                    progress = 0f;
                
                    if (timer >= _Time)
                    {
                        timer = 0f;
                        
                        if (this.subType == Define.TileType_Sub.Moving ||
                            this.bossFunc == Define.TileType_Sub.Moving)
                        {
                            // 타일이 끝으로 완전히 간 다음 Position을 바꿔주고 싶어서 이렇게 구현
                            if (this.tileActor != null)
                            {
                                this.isMovingDone = false;
                                var task = this.tileActor.Act(this, this.actCts);
                                this.actDisposable = task.ToObservable().Subscribe(x =>
                                {
                                    this.isMovingDone = true;
                                    IsFuncStart = x;
                                });

                                await UniTask.WaitUntil(() => this.isMovingDone == true, cancellationToken: _Cts.Token);

                                this.isMovingDone = false;
                                Vector2 changePos = new Vector2(this.transform.localPosition.x,
                                    this.transform.localPosition.y);
                                var posTuple = GetStartEndPosition(changePos);
                                this.startPos = posTuple._Start;
                                this.endPos = posTuple._End;
                        
                                this.originPos = changePos;
                                
                                continue;
                            }
                        }
                    }
                    
                    (this.startPos, this.endPos) = (this.endPos, this.startPos);
                }
            
                await UniTask.Yield(/*cancellationToken: _Cts.Token*/);
            }
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            Debug.LogError($"### Monster Move exception : {ex.Message} / {ex.StackTrace}");
        }

        IsFuncStart = false;
    }


    public override async UniTaskVoid TileTrigger()
    {
        Debug.Log($"MonsterType : {this.info.SubType}_{this.info.SubTypeIndex} 닿음");

        TileCollider.enabled = false;
        // TODO : 닿은 효과 await
        this.spriteRenderer.color = Color.blue;

        if (this.tileActor == null)
        {
            // Default 등 Func가 따로 없는 타일들을 위한
            IsFuncStart = false;
        }
        else
        {
            ActClear();
            this.tileActor = null;
        }
        
        await UniTask.WaitUntil(() => IsFuncStart == false);
        
        SoundManager.Instance.PlayOneShot(Define.SoundPath.SFX_MONSTER.ToString()).Forget();

        // LimitTry Stage인 경우 Try 횟수 1 감소
        if (GameManager.Instance.StageManager.StageInfo.Type == Define.StageType.LimitTry)
        {
            GameManager.Instance.StageManager.ChangeStageValue(-1);
        }

        // 몬스터에 닿으면 다시 처음부터 시작해야 함 -> fade 걷히는 처리
        GameManager.Instance.MapManager.IsFade.Value = false;

        // 되돌리는 애니메이션 Play await
        // TODO : 다시 처음 스테이지 상태로 돌리기 -> 처음 스테이지 구성될 때 타일 위치들을 json으로 저장해야 함!
        GameManager.Instance.RewindStage();
    }

    private (Vector2 _Start, Vector2 _End) GetStartEndPosition(Vector2 _Pos)
    {
        Vector2 startPosition = Vector2.zero;
        Vector2 endPosition = Vector2.zero;

        
        if (Mathf.Approximately(_Pos.x, (float)Define.MapSize.In_XStart) || Mathf.Approximately(_Pos.x, (float)Define.MapSize.In_XEnd))
        {
            if (Mathf.Approximately(_Pos.x, (float)Define.MapSize.In_XStart))
            {
                // 원래 In 안에서만 움직이게 하려고 했는데 (맵 내부), 너무 어려워서 내부를 넘어서 맵 outline까지 움직이도록 +1 더해줌
                startPosition = new Vector2((float)Define.MapSize.In_XEnd + 1f, _Pos.y);    
                endPosition = new Vector2(0f, _Pos.y);
            }
            else
            {
                startPosition = new Vector2(0f, _Pos.y);
                endPosition = new Vector2((float)Define.MapSize.In_XEnd + 1f, _Pos.y);
            }
        }
        else if (Mathf.Approximately(_Pos.y, (float)Define.MapSize.In_YStart) || Mathf.Approximately(_Pos.y, (float)Define.MapSize.In_YEnd))
        {
            if (Mathf.Approximately(_Pos.y, (float)Define.MapSize.In_YStart))
            {
                startPosition = new Vector2(_Pos.x, (float)Define.MapSize.Out_YEnd);
                endPosition = new Vector2(_Pos.x, (float)Define.MapSize.Out_YStart);
            }
            else
            {
                startPosition = new Vector2(_Pos.x, (float)Define.MapSize.Out_YStart);
                endPosition = new Vector2(_Pos.x, (float)Define.MapSize.Out_YEnd);
            }
        }

        return (startPosition, endPosition);
    }

    private void ActClear()
    {
        if (IsFuncStart == true)
        {
            Debug.Log("ActClear 에서 IsFuncStart false 처리");
            IsFuncStart = false;
        }

        if (this.actCts != null || this.actCts?.IsCancellationRequested == false)
        {
            this.actCts.Cancel();
        }

        if (this.moveCts != null || this.moveCts?.IsCancellationRequested == false)
        {
            this.moveCts.Cancel();
        }

        this.actDisposable?.Dispose();
    }
    
    private void OnDestroy()
    {
        ActClear();

        this.actCts?.Dispose();
        this.moveCts?.Dispose();
    }
}
