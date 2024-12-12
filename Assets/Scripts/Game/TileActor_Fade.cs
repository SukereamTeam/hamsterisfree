using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;
using FadeTweener = DG.Tweening.Core.TweenerCore<UnityEngine.Color, UnityEngine.Color, DG.Tweening.Plugins.Options.ColorOptions>;

public class TileActor_Fade : ITileActor
{
    private FadeTweener tweener = null;

    public async UniTask<bool> Act(TileBase tile, CancellationTokenSource cts, float activeTime = 0f)
    {
        try
        {
            while(cts.IsCancellationRequested == false)
            {
                // while문이 종료되어도 Delay는 진행 중에 취소되지 않기 때문에, 취소 토큰 넣어줘야 함
                await UniTask.Delay(TimeSpan.FromSeconds(activeTime), cancellationToken: cts.Token);

                tweener = tile.SpriteRenderer.DOFade(0f, TileBase.TILE_FADE_TIME).OnComplete(() =>
                {
                    tile.TileCollider.enabled = false;
                });

                await tweener;

                await UniTask.Delay(TimeSpan.FromSeconds(activeTime), cancellationToken: cts.Token);

                tile.TileCollider.enabled = true;       // 조금이라도 보이면 충돌체크 될 수 있도록

                tweener = tile.SpriteRenderer.DOFade(1f, TileBase.TILE_FADE_TIME);

                await tweener;
            }
        }
        catch (Exception ex)// when (!(ex is OperationCanceledException))
        {
            if (ex is OperationCanceledException)
            {
                Debug.Log($"Fade Token Cancel : {ex.Message} / {ex.StackTrace} //");

                tweener.Kill(true);
                tile.SpriteRenderer.color = Color.white;
            }
            else
            {
                Debug.Log($"### Tile Fade Error : {ex.Message} / {ex.StackTrace} //");
            }
        }

        return false;
    }
}
