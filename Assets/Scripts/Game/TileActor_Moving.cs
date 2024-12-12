using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class TileActor_Moving : ITileActor
{
    public async UniTask<bool> Act(TileBase tile, CancellationTokenSource cts, float activeTime = 0f)
    {
        try
        {
            while(cts.IsCancellationRequested == false)
            {
                // 다음 좌표 가져오기
                var nextData = GameManager.Instance.MapManager.GetRandomPosition_Next(tile.Info.Type);

                if (activeTime > 0f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(activeTime), cancellationToken: cts.Token);
                }
                
                // TODO : 이동 Effect

                // 다음 좌표로 이동, 다음 좌표를 _Tile의 TileInfo.Pos로 넣어주기(RootIdx도)
                tile.SetPosition(nextData.rootIdx, nextData.pos);
                
                if (tile.Info.Type == Define.TileType.Monster)
                {
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            if (ex is OperationCanceledException)
            {
                Debug.Log($"Moving Token Cancel : {ex.Message} / {ex.StackTrace} //");
            }
            else
            {
                Debug.Log($"### Tile Moving Error : {ex.Message} / {ex.StackTrace}");
            }
        }

        return false;
    }
}
