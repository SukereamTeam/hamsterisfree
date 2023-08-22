using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// partial class 로 분리하여
// SeedTile의 각 타입별 기능 구현만 모아놓은 스크립트 파일
public partial class SeedTile : TileBase
{
    private async UniTaskVoid Func_Fade(float _ActiveTime)
    {
        while (this.isFuncStart)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_ActiveTime));

            var fadeOut = this.spriteRenderer.DOFade(0f, FADE_TIME).OnComplete(() =>
            {
                this.tileCollider.enabled = false;
            });
            await fadeOut;

            await UniTask.Delay(TimeSpan.FromSeconds(_ActiveTime));

            this.tileCollider.enabled = true;       // 조금이라도 보이면 충돌체크 될 수 있도록
            var fadeIn = this.spriteRenderer.DOFade(1f, FADE_TIME);
            await fadeIn;
        }
    }


    private async UniTaskVoid Func_Disappear(float _ActiveTime)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(_ActiveTime));

        // TODO : 타일 사라지는 Ani 출력? 파티클도 출력?

        this.tileCollider.enabled = false;
        this.spriteRenderer.enabled = false;
    }


    private async UniTaskVoid Func_Moving(float _ActiveTime)
    {
        while (this.isFuncStart)
        {
            //GameManager.Instance.MapManager.GetRandomPos_Next(this);

            await UniTask.Delay(TimeSpan.FromSeconds(5f));

            // TODO : 자리에서 사라지는 파티클? 애니? 재생하고 다음 포지션으로 이동하기


        }
    }
}
