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
    private bool isFuncStart = false;
    private bool isMovingDone = false;

    private Vector2 originPos = new Vector2();
    private Vector2 startPos = new Vector2();
    private Vector2 endPos = new Vector2();
    
    private Define.MonsterTile_Direction directionFlag = Define.MonsterTile_Direction.NONE;
    private Table_Monster.Param monsterData;
    
    private CancellationTokenSource moveCts;
    private CancellationTokenSource actCts;
    private IDisposable disposable = null;

    private new void Start()
    {
        base.Start();

        this.actCts = new CancellationTokenSource();
        this.moveCts = new CancellationTokenSource();
    }

    public override void Initialize(TileInfo _Info, Vector2 _Pos)
    {
        var posTuple = GetStartEndPosition(_Pos);
        this.startPos = posTuple._Start;
        this.endPos = posTuple._End;

        this.originPos = _Pos;
        
        base.Initialize(_Info, this.startPos);
        
        this.subType = (Define.TileType_Sub)Enum.Parse(typeof(Define.TileType_Sub), _Info.SubType);

        var sprite = DataContainer.Instance.MonsterSprites[this.info.SubType];
        if (sprite != null)
        {
            this.spriteRenderer.sprite = sprite;
            this.spriteRenderer.color = Color.cyan;
        }
        
        this.monsterData = DataContainer.Instance.MonsterTable.GetParamFromType(
            this.info.SubType, (int)this.info.SubTypeIndex);

        TileFuncStart().Forget();
    }

    private async UniTaskVoid TileFuncStart()
    {
        // 스테이지 세팅 끝나고 게임 시작할 상태가 되었을 때(IsGame == true)
        // 그 때 타일 타입마다 부여된 액션 실행
        await UniTask.WaitUntil(() => GameManager.Instance?.IsGame.Value == true);

        Move(this.info.ActiveTime, this.monsterData.MoveSpeed, this.moveCts).Forget();
        
        if (this.isFuncStart == false)
        {
            Debug.Log("Monster Func Start !!!");
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
            if (this.subType == Define.TileType_Sub.Moving)
            {
                return;
            }
            
            var task = this.tileActor.Act(this, this.actCts, this.info.ActiveTime);
            this.disposable = task.ToObservable().Subscribe(x =>
            {
                this.isFuncStart = x;
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
                        
                        if (this.subType == Define.TileType_Sub.Moving)
                        {
                            // 타일이 끝으로 완전히 간 다음 Position을 바꿔주고 싶어서 이렇게 구현
                            if (this.tileActor != null)
                            {
                                this.isMovingDone = false;
                                var task = this.tileActor.Act(this, this.actCts);
                                this.disposable = task.ToObservable().Subscribe(x =>
                                {
                                    this.isMovingDone = true;
                                    this.isFuncStart = x;
                                });

                                await UniTask.WaitUntil(() => this.isMovingDone == true);

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
            
                await UniTask.Yield();
            }
        }
        catch (Exception ex)
        {
            // Cancel 토큰으로 종료되었을 때
            if (ex is OperationCanceledException)
            {
                Debug.Log("### MonsterTile Move ---> " + ex.Message + " ###");
            }
            else
            {
                Debug.Log("### MonsterTile Move Error : " + ex.Message + " ###");
            }
        }
    }


    public override async UniTaskVoid TileTrigger()
    {
        Debug.Log($"SeedType : {this.info.SubType}, SeedValue : {this.info.SeedValue} 먹음");

        this.tileCollider.enabled = false;

    }

    private (Vector2 _Start, Vector2 _End) GetStartEndPosition(Vector2 _Pos)
    {
        Vector2 startPosition = Vector2.zero;
        Vector2 endPosition = Vector2.zero;

        if (Mathf.Approximately(_Pos.x, 1f) || Mathf.Approximately(_Pos.x, 6f))
        {
            startPosition = new Vector2(Mathf.Approximately(_Pos.x, 1f) ? 7f : 0f, _Pos.y);
            endPosition = new Vector2(Mathf.Approximately(_Pos.x, 1f) ? 0f : 7f, _Pos.y);
        }
        else if (Mathf.Approximately(_Pos.y, 0f) || Mathf.Approximately(_Pos.y, 8f))
        {
            startPosition = new Vector2(_Pos.x, Mathf.Approximately(_Pos.y, 0f) ? 9f : -1f);
            endPosition = new Vector2(_Pos.x, Mathf.Approximately(_Pos.y, 0f) ? -1f : 9f);
        }

        return (startPosition, endPosition);
    }
    
    private void OnDestroy()
    {
        this.actCts.Cancel();
        this.moveCts.Cancel();
        
        if (this.disposable != null)
        {
            Debug.Log("Dispose!!!");
            this.disposable.Dispose();
        }
    }
}
