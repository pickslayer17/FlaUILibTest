using System.Collections.Concurrent;

namespace UIDriver;

public sealed class WindowManager
{
    private readonly AutomationElementObject _window;
    private readonly Watcher _watcher;
    private readonly ConcurrentDictionary<Guid, Order> _orders = new();

    public WindowManager(AutomationElementObject window, Watcher watcher)
    {
        _window = window;
        _watcher = watcher;
    }

    public async Task<AutomationElementObject> ServeOrderAsync(Order order)
    {
        _orders.TryAdd(Guid.NewGuid(), order);
        var finder = FinderFabric.GetFinder(order.By);
        why not async?
        return await _watcher.ProcessOrder(order, finder, _window);
    }
}
