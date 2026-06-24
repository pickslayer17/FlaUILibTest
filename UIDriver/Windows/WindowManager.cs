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

    public Task<AutomationElementObject> ProcessOrderAsync(Order order)
    {
        _orders.TryAdd(order.Id, order);
        var finder = FinderFactory.GetFinder(order.By);
        return _watcher.ExecuteOrderAsync(order, finder, _window);
    }
}
