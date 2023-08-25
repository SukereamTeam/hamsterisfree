using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using FadeTweener = DG.Tweening.Core.TweenerCore<UnityEngine.Color, UnityEngine.Color, DG.Tweening.Plugins.Options.ColorOptions>;


public class TileActor_Disappear : ITileActor
{
    private FadeTweener tweener = null;

    public async UniTask<bool> Act(TileBase _Tile, float _ActiveTime = 0, CancellationTokenSource _Cts = default)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_ActiveTime), cancellationToken: _Cts.Token);

            // TODO : 타일 사라지는 Ani 출력? 파티클도 출력?

            _Tile.TileCollider.enabled = false;

            this.tweener = _Tile.SpriteRenderer.DOFade(0f, TileBase.FADE_TIME);
            
            await tweener;
        }
        catch (Exception ex)
        {
            // Cancel 토큰으로 종료되었을 때
            if (ex is OperationCanceledException)
            {
                Debug.Log("### Tile Disappear ---> Cancel " + ex.Message + " ###");

                // 트윈 삭제
                tweener.Kill(true);
            }
            else
            {
                Debug.Log("### Tile Fade Error : " + ex.Message + " ###");
            }
        }

        return false;
    }
}
