using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace UIDriver;

public sealed class Watcher
{
    private readonly ConcurrentDictionary<Guid, Watch> _watches = new();

    public async Task<AutomationElementObject> ProcessOrder(Order order, IFinder finder, AutomationElementObject source)
    {should we do everything async?? maybe you were //doing skeleton and just didnt care about it, but i found it veery sttrange, because i tried to understand logic and i failed, firstly it seemed cool, like locato awaits task, and all other guys just have link to the task, but after i realized that it will be a lot of fire and forge, but maybe it make sense
        var watch = AddWatch(finder, source);

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

    private Watch AddWatch(IFinder finder, AutomationElementObject source)
    {
        var watch = new Watch(finder, source);
        _watches.TryAdd(watch.Id, watch);

        return watch;
    }
}
