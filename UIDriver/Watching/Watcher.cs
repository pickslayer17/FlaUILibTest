using System.Collections.Concurrent;
using UIDriver.Matchers;

namespace UIDriver;

public sealed class Watcher
{
    private readonly ConcurrentDictionary<Guid, Watch> _watches = new();
    private readonly AutomationElementObject _windowSource;

    public Watcher(AutomationElementObject windowSource)
    {
        _windowSource = windowSource;
    }

    public async Task<AutomationElementObject> ExecuteOrderAsync(Order order, IFinder finder, IMatcher matcher)
    {
        var watch = CreateWatch(finder, matcher);
        var result = await WaitWatchAsync(watch, order.By.Timeout);
        CompleteWatch(watch, order);
        LogEventFactory.RaiseElementResolved(order.Id);

        return result;
    }

    public void PokeOnStructureChanged(AutomationElementObject source)
    {
        foreach (var (id, watch) in _watches)
        {
            if (watch.TryResolveFindDescendant(source))
            {
                _watches.TryRemove(id, out _);
            }
        }
    }

    public void PokeOnPropertyChanged(AutomationElementObject source)
    {
        foreach (var (id, watch) in _watches)
        {
            if (watch.TryResolveMatch(source))
            {
                _watches.TryRemove(id, out _);
            }
        }
    }

    private Watch CreateWatch(IFinder finder, IMatcher matcher)
    {
        var watch = new Watch(finder, matcher);
        _watches.TryAdd(watch.Id, watch);

        return watch;
    }

    private async Task<AutomationElementObject> WaitWatchAsync(Watch watch, TimeSpan timeout)
    {
        if (!watch.TryResolveFindDescendant(_windowSource))
            await watch.Task.WaitAsync(timeout);

        return await watch.Task;
    }

    private void CompleteWatch(Watch watch, Order order)
    {
        _watches.TryRemove(watch.Id, out _);
        order.Status = OrderStatus.Completed;
    }
}
