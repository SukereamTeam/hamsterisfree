using Cysharp.Threading.Tasks;
using System.Threading;

public interface ITileActor
{
    public abstract UniTask<bool> Act(TileBase tile, CancellationTokenSource cts = default, float activeTime = 0f);
}