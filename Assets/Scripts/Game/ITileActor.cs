using Cysharp.Threading.Tasks;
using System.Threading;

public interface ITileActor
{
    public abstract UniTask<bool> Act(TileBase _Tile, float _ActiveTime = 0f, CancellationTokenSource _Cts = default);
}