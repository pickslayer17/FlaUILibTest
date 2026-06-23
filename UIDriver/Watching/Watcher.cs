using System.Collections.Concurrent;

namespace UIDriver;

public sealed class Watcher
{
    private readonly ConcurrentDictionary<Guid, Watch> _watches = new();

    public async Task<AutomationElementObject> AddWatch(Order order, IFinder finder, AutomationElementObject source)
    {
        var watch = new Watch(finder, source);
        _watches.TryAdd(watch.Id, watch);

        if (!watch.TryResolve())
            await watch.Task.WaitAsync(order.By.Timeout);

        var result = await watch.Task;
        _watches.TryRemove(watch.Id, out _);
        order.Status = OrderStatus.Completed;
        return result;
    }

    public void Poke()
    {
        foreach (var (id, watch) in _watches)
            if (watch.TryResolve())
                _watches.TryRemove(id, out _);
    }
}
