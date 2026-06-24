using System.Collections.Concurrent;

namespace UIDriver;

public sealed class Watcher
{
    private readonly ConcurrentDictionary<Guid, Watch> _watches = new();

    public async Task<AutomationElementObject> ExecuteOrderAsync(Order order, IFinder finder, AutomationElementObject source)
    {
        var watch = CreateWatch(finder, source);
        var result = await AwaitWatchAsync(watch, order.By.Timeout);
        CompleteWatch(watch, order);

        return result;
    }

    public void Poke()
    {
        foreach (var (id, watch) in _watches)
            if (watch.TryResolve())
                _watches.TryRemove(id, out _);
    }

    private Watch CreateWatch(IFinder finder, AutomationElementObject source)
    {
        var watch = new Watch(finder, source);
        _watches.TryAdd(watch.Id, watch);

        return watch;
    }

    private static async Task<AutomationElementObject> AwaitWatchAsync(Watch watch, TimeSpan timeout)
    {
        if (!watch.TryResolve())
            await watch.Task.WaitAsync(timeout);

        return await watch.Task;
    }

    private void CompleteWatch(Watch watch, Order order)
    {
        _watches.TryRemove(watch.Id, out _);
        order.Status = OrderStatus.Completed;
    }
}
