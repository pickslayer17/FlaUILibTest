using System.Collections.Concurrent;
using UIDriver.Matchers;

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
        if(!_orders.TryAdd(order.Id, order))
            throw new InvalidOperationException($"Order with id {order.Id} is already registered.");

        var finder = FinderFactory.GetFinder(order.By);
        var matcher = MatcherFactory.GetMatcher(order.By);

        return _watcher.ExecuteOrderAsync(order, finder, matcher);
    }
}
