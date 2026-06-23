using System.Collections.Concurrent;
using FlaUI.Core.AutomationElements;

namespace UIDriver;

public sealed class WindowManager
{
    private readonly AutomationElement _window;
    private readonly Watcher _watcher;
    private readonly FinderFabric _fabric;
    private readonly ConcurrentDictionary<Guid, Order> _orders = new();

    public WindowManager(AutomationElement window, Watcher watcher, FinderFabric fabric)
    {
        _window = window;
        _watcher = watcher;
        _fabric = fabric;
    }

    public Task<AutomationElement> Handle(BY by) => throw new NotImplementedException();
}
