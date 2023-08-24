using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
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
                var nextData = GameManager.Instance.MapManager.GetRandomPosition_Next(_Tile);

                await UniTask.Delay(TimeSpan.FromSeconds(_ActiveTime));

                // TODO : 이동 Effect

                // 다음 좌표로 이동, 다음 좌표를 _Tile의 TileInfo.Pos로 넣어주기(RootIdx도)
                _Tile.SetPosition(nextData.rootIdx, nextData.pos);


                // TODO : 이동 완료 대기
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
