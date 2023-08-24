using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public interface ITileActor
{
    public abstract UniTask<bool> Act(TileBase _Tile, float _ActiveTime = 0f, CancellationTokenSource _CancellationToken = default);
}