using System.Collections.Concurrent;
using FlaUI.Core.AutomationElements;

namespace UIDriver;

// Оркестратор ВНУТРИ одного окна. Принимает BY, берёт у фабрики нужный IFinder, отдаёт ему
// source (своё окно) и заводит Watch в Watcher. Держит свои Orders, которых ждёт AppManager.
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

    // BY → IFinder (фабрика) → Watcher.AddWatch(finder, _window, by.Timeout).
    public Task<AutomationElement> Handle(BY by) => throw new NotImplementedException();
}
