using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;
using FadeTweener = DG.Tweening.Core.TweenerCore<UnityEngine.Color, UnityEngine.Color, DG.Tweening.Plugins.Options.ColorOptions>;

public class TileActor_Fade : ITileActor
{
    private FadeTweener tweener = null;

    public async UniTask<bool> Act(TileBase _Tile, float _ActiveTime = 0f, CancellationTokenSource _Cts = default)
    {
        try
        {
            while(_Cts.IsCancellationRequested == false)
            {
                // while문이 종료되어도 Delay는 진행 중에 취소되지 않기 때문에, 취소 토큰 넣어줘야 함
                await UniTask.Delay(TimeSpan.FromSeconds(_ActiveTime), cancellationToken: _Cts.Token);

                this.tweener = _Tile.SpriteRenderer.DOFade(0f, TileBase.FADE_TIME).OnComplete(() =>
                {
                    _Tile.TileCollider.enabled = false;
                });

                await this.tweener;

                await UniTask.Delay(TimeSpan.FromSeconds(_ActiveTime), cancellationToken: _Cts.Token);

                _Tile.TileCollider.enabled = true;       // 조금이라도 보이면 충돌체크 될 수 있도록

                this.tweener = _Tile.SpriteRenderer.DOFade(1f, TileBase.FADE_TIME);

                await this.tweener;
            }
        }
        catch (Exception ex)
        {
            // Cancel 토큰으로 종료되었을 때
            if (ex is OperationCanceledException)
            {
                Debug.Log("### Tile Fade ---> Cancel " + ex.Message + " ###");

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
