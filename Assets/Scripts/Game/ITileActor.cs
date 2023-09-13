using Cysharp.Threading.Tasks;
using System.Threading;

public interface ITileActor
{
    public abstract UniTask<bool> Act(TileBase _Tile, CancellationTokenSource _Cts = default, float _ActiveTime = 0f);
}