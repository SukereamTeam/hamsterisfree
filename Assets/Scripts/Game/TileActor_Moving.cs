using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class TileActor_Moving : ITileActor
{
    public async UniTask<bool> Act(TileBase _Tile, CancellationTokenSource _Cts, float _ActiveTime = 0f)
    {
        try
        {
            while(_Cts.IsCancellationRequested == false)
            {
                // 다음 좌표 가져오기
                var nextData = GameManager.Instance.MapManager.GetRandomPosition_Next(_Tile.Info.Type);

                if (_ActiveTime > 0f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(_ActiveTime), cancellationToken: _Cts.Token);
                }
                
                // TODO : 이동 Effect

                // 다음 좌표로 이동, 다음 좌표를 _Tile의 TileInfo.Pos로 넣어주기(RootIdx도)
                _Tile.SetPosition(nextData.rootIdx, nextData.pos);
                
                if (_Tile.Info.Type == Define.TileType.Monster)
                {
                    return true;
                }
            }

            if (_Cts.IsCancellationRequested == true)
            {
                Debug.Log("Cancel 토큰으로 취소됨!");
            }
        }
        catch (Exception ex)
        {
            // Cancel 토큰으로 종료되었을 때
            //if (ex is OperationCanceledException)
            //{
            //    Debug.Log("### Tile Moving ---> Cancel " + ex.Message + " ###");
            //}
            //else
            //{
                Debug.Log("### Tile Moving Error : " + ex.Message + " ###");
            //}
        }

        return false;
    }
}
