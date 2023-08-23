using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public partial class SeedTile : TileBase
{
    private bool isTouch = false;
    public bool IsTouch {
        get => this.isTouch;
        set => this.isTouch = value;
    }

    [SerializeField]
    private Define.SeedTile_Type tileType = Define.SeedTile_Type.Default;

    private bool isFuncStart = false;


//    interface ITileActor
//    {
//        async act()
//}

//    class FadeActor : ITileActor
//    {
//        async act()
//    }

//...

//-------------------------------



    private ITileActor tileActor;


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
        // 스테이지 세팅 끝나고 게임 시작할 상태가 되었을 때, 타일 타입마다 부여된 액션 실행
        await UniTask.WaitUntil(() => GameManager.Instance.IsGame.Value == true);

        if (this.isFuncStart == false)
        {
            Debug.Log("SeedTile Func Start!");
            this.isFuncStart = true;
        }

        switch (this.tileType)
        {
            case Define.SeedTile_Type.Disappear:
                {
                    tileActor = new FadeActor();
                    await tileActor.Act();
                    Func_Disappear(this.info.ActiveTime).Forget();
                }break;
            case Define.SeedTile_Type.Moving:
                {
                    Func_Moving(this.info.ActiveTime).Forget();
                }break;
            case Define.SeedTile_Type.Fade:
                {
                    Func_Fade(this.info.ActiveTime).Forget();
                }
                break;
        }
    }


    

    public override void TileTriggerEvent()
    {
        Debug.Log("Seed 먹음");

        this.isFuncStart = false;

        this.tileCollider.enabled = false;

        // TODO : Delete... 테스트 용도
        this.spriteRenderer.color = Color.red;

        GameManager.Instance.SeedCount++;

        // TODO
        // Trigger Ani 재생
        
    }

}
