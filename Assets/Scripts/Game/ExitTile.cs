using Cysharp.Threading.Tasks;
using UnityEngine;

public class ExitTile : TileBase
{
    public override void Initialize(TileInfo _Info, Vector2 _Pos)
    {
        base.Initialize(_Info, _Pos);

        var sprite = DataContainer.Instance.ExitSprite;
        if (sprite != null)
        {
            this.spriteRenderer.sprite = sprite;
        }
    }

    public override async UniTaskVoid TileTrigger()
    {
        GameManager.Instance.MapManager.IsFade.Value = false;
        GameManager.Instance.IsGame.Value = false;      // 임시 게임종료 처리

        await UniTask.CompletedTask;
    }
}
