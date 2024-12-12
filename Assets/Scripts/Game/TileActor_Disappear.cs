using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;
using FadeTweener = DG.Tweening.Core.TweenerCore<UnityEngine.Color, UnityEngine.Color, DG.Tweening.Plugins.Options.ColorOptions>;


public class TileActor_Disappear : ITileActor
{
    private FadeTweener _tweener = null;

    public async UniTask<bool> Act(TileBase tile, CancellationTokenSource cts, float activeTime = 0)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(activeTime), cancellationToken: cts.Token);

            // TODO : 타일 사라지는 Ani 출력? 파티클도 출력?

            tile.TileCollider.enabled = false;

            _tweener = tile.SpriteRenderer.DOFade(0f, TileBase.TILE_FADE_TIME);
            
            await _tweener;
        }
        catch (Exception ex)
        {
            if (ex is OperationCanceledException)
            {
                Debug.Log($"Disappear Token Cancel : {ex.Message} / {ex.StackTrace} //");

                _tweener.Kill(true);
                tile.SpriteRenderer.color = Color.white;
            }
            else
            {
                Debug.Log($"### Tile Disappear Error : {ex.Message} / {ex.StackTrace} //");
            }
        }

        return false;
    }
}
