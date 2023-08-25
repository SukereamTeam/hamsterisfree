using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;

public class SeedTile : TileBase
{
    private bool isTouch = false;
    public bool IsTouch {
        get => this.isTouch;
        set => this.isTouch = value;
    }

    [SerializeField]
    private Define.SeedTile_Type tileType = Define.SeedTile_Type.Default;

    
    private ITileActor tileActor;

    private bool isFuncStart = false;
    private CancellationTokenSource cts;
    private IDisposable disposable = null;




    private new void Start()
    {
        base.Start();

        this.cts = new CancellationTokenSource();
    }

    public override void Initialize(TileInfo _Info)
    {
        base.Initialize(_Info);

        this.tileType = (Define.SeedTile_Type)Enum.Parse(typeof(Define.SeedTile_Type), _Info.SubType);

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
        await UniTask.WaitUntil(() => GameManager.Instance.IsGame.Value == true);

        if (this.isFuncStart == false)
        {
            Debug.Log("SeedTile Func Start !!!");
            this.isFuncStart = true;
        }

        if (this.tileActor != null)
        {
            this.tileActor = null;
        }

        switch (this.tileType)
        {
            case Define.SeedTile_Type.Disappear:
                {
                    this.tileActor = new TileActor_Disappear();
                    
                }break;
            case Define.SeedTile_Type.Moving:
                {
                    this.tileActor = new TileActor_Moving();
                    //this.isFuncStart = await this.tileActor.Act(this, this.info.ActiveTime, this.cts);
                }break;
            case Define.SeedTile_Type.Fade:
                {
                    this.tileActor = new TileActor_Fade();
                    //this.isFuncStart = await this.tileActor.Act(this, this.info.ActiveTime, this.cts);
                }break;
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


    

    public override async UniTaskVoid TileTriggerEvent()
    {
        Debug.Log("Seed 먹음");

        this.tileCollider.enabled = false;
        GameManager.Instance.SeedCount++;

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
