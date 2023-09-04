using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class TileActor_Moving : ITileActor
{
    public async UniTask<bool> Act(TileBase _Tile, float _ActiveTime = 0, CancellationTokenSource _Cts = default)
    {
        try
        {
            while(_Cts.IsCancellationRequested == false)
            {
                // 다음 좌표 가져오기
                var nextData = GameManager.Instance.MapManager.GetRandomPosition_Next(_Tile.Info.Type);

                await UniTask.Delay(TimeSpan.FromSeconds(_ActiveTime), cancellationToken: _Cts.Token);

                // TODO : Monster 일 경우 끝까지 다 간 다음에 이동시키고 싶음
                // 끝에 다 다르면 (Move 함수에서 t == 1) true가 되고, 이 값이 true일 때 움직이게 하고 싶음
                
                
                
                // TODO : 이동 Effect

                // 다음 좌표로 이동, 다음 좌표를 _Tile의 TileInfo.Pos로 넣어주기(RootIdx도)
                _Tile.SetPosition(nextData.rootIdx, nextData.pos);
            }

        }
        catch (Exception ex)
        {
            // Cancel 토큰으로 종료되었을 때
            if (ex is OperationCanceledException)
            {
                Debug.Log("### Tile Moving ---> Cancel " + ex.Message + " ###");
            }
            else
            {
                Debug.Log("### Tile Moving Error : " + ex.Message + " ###");
            }
        }

        return false;
    }
}
