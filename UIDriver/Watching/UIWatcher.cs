using System.Collections.Concurrent;
using UIDriver.Interfaces;
using UIDriver.Matchers;

namespace UIDriver;

public sealed class UIWatcher : IStructureChangedListener, IPropertyChangedListener
{
    private readonly ConcurrentDictionary<Guid, UIWatch> _watches = new();
    private readonly UIAutomationElement _windowSource;

    public UIWatcher(UIAutomationElement windowSource)
    {
        _windowSource = windowSource;
    }

    public async Task<UIAutomationElement> ExecuteOrderAsync(Order order, IFinder finder, IMatcher matcher)
    {
        var watch = CreateWatch(finder, matcher);
        var result = await WaitWatchAsync(watch, order.By.Timeout);
        CompleteWatch(watch, order);
        LogEventFactory.RaiseElementResolved(order.Id);

        return result;
    }

    public void NotifyOnStructureChanged(UIAutomationElement source)
    {
        foreach (var (id, watch) in _watches)
        {
            if (watch.TryResolveFindDescendant(source))
            {
                _watches.TryRemove(id, out _);
            }
        }
    }

    public void NotifyOnPropertyChanged(UIAutomationElement source)
    {
        foreach (var (id, watch) in _watches)
        {
            if (watch.TryResolveMatch(source))
            {
                _watches.TryRemove(id, out _);
            }
        }
    }

    private UIWatch CreateWatch(IFinder finder, IMatcher matcher)
    {
        var watch = new UIWatch(finder, matcher);
        _watches.TryAdd(watch.Id, watch);

        return watch;
    }

    private async Task<UIAutomationElement> WaitWatchAsync(UIWatch watch, TimeSpan timeout)
    {
        if (!watch.TryResolveFindDescendant(_windowSource))
            await watch.Task.WaitAsync(timeout); // add exeptions for timeouts

        return await watch.Task;
    }

    private void CompleteWatch(UIWatch watch, Order order)
    {
        _watches.TryRemove(watch.Id, out _);
        order.Status = OrderStatus.Completed;
    }
}
