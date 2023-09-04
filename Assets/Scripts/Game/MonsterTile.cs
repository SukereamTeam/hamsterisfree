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
        
        // if (this.isFuncStart == false)
        // {
        //     Debug.Log("SeedTile Func Start !!!");
        //     this.isFuncStart = true;
        // }
        //
        // if (this.tileActor != null)
        // {
        //     this.tileActor = null;
        // }
        //
        // switch (this.subType)
        // {
        //     case Define.TileType_Sub.Disappear:
        //     {
        //         this.tileActor = new TileActor_Disappear();
        //     }
        //         break;
        //     case Define.TileType_Sub.Moving:
        //     {
        //         this.tileActor = new TileActor_Moving();
        //     }
        //         break;
        //     case Define.TileType_Sub.Fade:
        //     {
        //         this.tileActor = new TileActor_Fade();
        //     }
        //         break;
        // }
        //
        // if (this.tileActor != null)
        // {
        //     var task = this.tileActor.Act(this, this.info.ActiveTime, this.actCts);
        //     this.disposable = task.ToObservable().Subscribe(x =>
        //     {
        //         this.isFuncStart = x;
        //     });
        // }
        
    }

    private async UniTaskVoid Move(float _Time, float _Speed, CancellationTokenSource _Cts)
    {
        float t = 0f;
        while (this.moveCts.IsCancellationRequested == false)
        {
            //float elapsedTime = 0f;

            t += Time.deltaTime * _Speed; //elapsedTime / _Time;
            t = Mathf.Clamp01(t);       // 0~1 사이 값 유지
            
            var newPosition = Vector3.Lerp(this.startPos, this.endPos, t);
            
            this.transform.localPosition = newPosition;
            
            //elapsedTime += Time.deltaTime;
            
            if (t >= 1f)
            {
                t = 0f;
                    
                (this.startPos, this.endPos) = (this.endPos, this.startPos);
            }
            
            await UniTask.Yield();
        }
    }


    public override async UniTaskVoid TileTrigger()
    {
        Debug.Log($"SeedType : {this.info.SubType}, SeedValue : {this.info.SeedValue} 먹음");

        this.tileCollider.enabled = false;

    }

    private (Vector2 _Start, Vector2 _End) GetStartEndPosition(Vector2 _Pos)
    {
        var startPosition = new Vector2(_Pos.x, _Pos.y);
        var endPosition = Vector2.zero;
        
        // TODO : 같은 타일이 뽑힐 때가 있음... Monster 일 때...
        
        // 양 옆으로 이동
        if (_Pos.x == 1f || _Pos.x == 6f)
        {
            // 기존엔 맵 안에서만 이동했는데, 어려운 것 같아 Outline Tile까지 이동하도록 좌표를 1씩 빼주고 더해줌
            if (_Pos.x > 1)
            {
                startPosition = new Vector2(7f, this.transform.position.y);
                endPosition = new Vector2(0f, this.transform.position.y);
            }
            else
            {
                startPosition = new Vector2(0f, this.transform.position.y);
                endPosition = new Vector2(7f, this.transform.position.y);
            }
        }
        // 위 아래로 이동
        else if (_Pos.y == 0f || _Pos.y == 8f)
        {
            if (_Pos.y > 0f)
            {
                startPosition = new Vector2(this.transform.position.x, 9f);
                endPosition = new Vector2(this.transform.position.x, -1f);
            }
            else
            {
                startPosition = new Vector2(this.transform.position.x, -1f);
                endPosition = new Vector2(this.transform.position.x, 9f);
            }
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
