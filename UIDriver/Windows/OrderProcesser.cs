using System.Collections.Concurrent;
using UIDriver.Matchers;

namespace UIDriver;

public sealed class OrderProcesser
{
    private readonly UIWatcher _watcher;
    private readonly ConcurrentDictionary<Guid, Order> _orders = new();

    public OrderProcesser(UIWatcher watcher)
    {
        _watcher = watcher;
    }

    public Task<UIAutomationElement> ProcessOrderAsync(Order order)
    {
        if(!_orders.TryAdd(order.Id, order))
            throw new InvalidOperationException($"Order with id {order.Id} is already registered.");

        var finder = UIFinderFactory.GetFinder(order.By);
        var matcher = MatcherFactory.GetMatcher(order.By);

        return _watcher.ExecuteOrderAsync(order, finder, matcher);
    }
}
