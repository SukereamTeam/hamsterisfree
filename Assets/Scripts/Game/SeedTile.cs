using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using DG.Tweening;

public class SeedTile : TileBase
{

    private bool isTouch = false;
    public bool IsTouch {
        get => this.isTouch;
        set => this.isTouch = value;
    }

    private readonly Subject<Unit> actionSubject = new Subject<Unit>();
    private bool isActionRunning = false;




    public override void Initialize(TileInfo _Info)
    {
        base.Initialize(_Info);

        var sprite = DataContainer.Instance.SeedSprites[this.info.SubType];

        if (sprite != null)
        {
            this.spriteRenderer.sprite = sprite;
        }

        switch(this.info.SubType)
        {
            case "Fade":
                {
                    StartFadeAction();
                }
                break;
        }
    }

    private void StartFadeAction()
    {
        // Fade 타입의 동작을 시작
        if (isActionRunning == false)
        {
            isActionRunning = true;
            Observable.Start(() =>
            {
                // Fade 타입의 동작 코드
                Debug.Log("Fade 타입 동작 시작");
                // 여기에서 필요한 동작 수행

                var sequence = DOTween.Sequence()
            .Append(spriteRenderer.DOFade(0f, 0.5f)) // FadeOut
            .AppendCallback(() => this.tileCollider.enabled = false) // Collider 비활성화
            .AppendInterval(0.5f) // 0.5초 대기
            .Append(spriteRenderer.DOFade(1f, 0.5f)) // FadeIn
            .AppendCallback(() => this.tileCollider.enabled = true) // Collider 활성화
            .AppendInterval(0.5f) // 0.5초 대기
            .SetLoops(-1); // 무한 루프

            }).DoOnCompleted(() =>
            {
                isActionRunning = false;
                actionSubject.OnCompleted(); // 동작이 끝나면 이벤트를 완료
            }).Subscribe();
        }
    }

    public override void TileTriggerEvent()
    {
        // Check 타일 지나감

        Debug.Log("Seed 먹음");

        this.tileCollider.enabled = false;

        // TODO : Delete... 테스트 용도
        this.spriteRenderer.color = Color.red;

        GameManager.Instance.SeedCount++;

        // TODO
        // Trigger Ani 재생
        
    }

}
