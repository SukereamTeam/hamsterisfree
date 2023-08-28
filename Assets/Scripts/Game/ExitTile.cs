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
        //Debug.Log("Game End");

        // 별 갯수 계산해서
        // 완료 처리 or 무반응

        GameManager.Instance.IsGame.Value = false;      // 임시 게임종료 처리

        await UniTask.CompletedTask;
    }
}
