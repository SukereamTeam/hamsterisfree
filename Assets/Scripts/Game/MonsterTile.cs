using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UniRx;
using UnityEngine.Serialization;

public class MonsterTile : TileBase
{
    [SerializeField]
    private Define.TileType_Sub subType = Define.TileType_Sub.Default;

    private ITileActor tileActor;
    private bool isFuncStart = false;
    private Vector2 Pos = new Vector2();
    private Define.MonsterTile_Direction directionFlag = Define.MonsterTile_Direction.NONE;

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
        base.Initialize(_Info, _Pos);
        this.Pos = _Pos;            // this.transform.localPosition 으로 바꿀 수 있지 않나?
        
        this.subType = (Define.TileType_Sub)Enum.Parse(typeof(Define.TileType_Sub), _Info.SubType);

        var sprite = DataContainer.Instance.MonsterSprites[this.info.SubType];
        if (sprite != null)
        {
            this.spriteRenderer.sprite = sprite;
            this.spriteRenderer.color = Color.cyan;
        }

        TileFuncStart().Forget();
    }

    private async UniTaskVoid TileFuncStart()
    {
        // 스테이지 세팅 끝나고 게임 시작할 상태가 되었을 때(IsGame == true)
        // 그 때 타일 타입마다 부여된 액션 실행
        await UniTask.WaitUntil(() => GameManager.Instance?.IsGame.Value == true);

        Move(this.info.ActiveTime, this.moveCts).Forget();
        
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

    private async UniTaskVoid Move(float _Time, CancellationTokenSource _Cts)
    {
        Vector3 startPosition = this.transform.position;
        Vector3 endPosition = Vector3.zero;

        while (this.moveCts.IsCancellationRequested == false)
        {
            // 양 옆으로 이동
            if (this.transform.position.x == 1 || this.transform.position.x == 6)
            {
                // 기존엔 맵 안에서만 이동했는데, 어려운 것 같아 Outline Tile까지 이동하도록 좌표를 1씩 빼주고 더해줌
                if (startPosition.x > 1)
                {
                    startPosition = new Vector3(7, this.transform.position.y, this.transform.position.z);
                    endPosition = new Vector3(0, this.transform.position.y, this.transform.position.z);
                }
                else
                {
                    startPosition = new Vector3(0, this.transform.position.y, this.transform.position.z);
                    endPosition = new Vector3(7, this.transform.position.y, this.transform.position.z);
                }
            }
            // 위 아래로 이동
            else if (this.transform.position.y == 0 || this.transform.position.y == 8)
            {
                if (startPosition.y > 0)
                {
                    startPosition = new Vector3(this.transform.position.x, 9, this.transform.position.z);
                    endPosition = new Vector3(this.transform.position.x, -1, this.transform.position.z);
                }
                else
                {
                    startPosition = new Vector3(this.transform.position.x, -1, this.transform.position.z);
                    endPosition = new Vector3(this.transform.position.x, 9, this.transform.position.z);
                }
            }

            float elapsedTime = 0f;

            while (this.moveCts.IsCancellationRequested == false && elapsedTime < _Time)
            {
                float t = elapsedTime / _Time;
                Vector3 newPosition = Vector3.Lerp(startPosition, endPosition, t);

                this.transform.position = newPosition;

                elapsedTime += Time.deltaTime;

                await UniTask.Yield();
            }

            (startPosition, endPosition) = (endPosition, startPosition);
        }
    }


    public override async UniTaskVoid TileTrigger()
    {
        Debug.Log($"SeedType : {this.info.SubType}, SeedValue : {this.info.SeedValue} 먹음");

        this.tileCollider.enabled = false;

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
