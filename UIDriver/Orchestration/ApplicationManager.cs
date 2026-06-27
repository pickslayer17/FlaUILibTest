using FlaUI.Core;
using FlaUI.Core.Identifiers;
using System.Collections.Concurrent;
using UIDriver.Constants;

namespace UIDriver;

public sealed class ApplicationManager
{
    public IEventLibrary EventLibrary => _automationBase.EventLibrary;

    private readonly AutomationBase _automationBase;
    private readonly ConcurrentDictionary<RunTimeId, WindowContainer> _containers = new();
    private readonly ConcurrentDictionary<Guid, Order> _orders = new();
    private readonly ToggleWindowListener _toggleWindowListener;

    private Lock _windowEventLock = new();

    private WindowContainer? _defaultContainer;
    private WindowContainer? _desktopContainer;

    public ApplicationManager(AutomationBase automation)
    {
        _automationBase = automation;
        _toggleWindowListener = new ToggleWindowListener(this);
    }

    public void RegisterDefault(AutomationElementObject window) => _defaultContainer = CreateWindowContainer(window);

    public void RegisterDesktop(AutomationElementObject window) => _desktopContainer = CreateWindowContainer(window);

    public Task<AutomationElementObject> RequestElementAsync(BY by)
    {
        var container = ResolveContainer(by);
        var order = RegisterOrder(by);

        var task = container.SubmitOrderAsync(order);
        order.Task = task;
        return task;
    }

    public void NotifyWindowOpened(AutomationElementObject window, EventId eventId)
    {
        lock (_windowEventLock)
        {
            if(window.RunTimeId.State != RunTimeIdStates.Valid)
                throw new InvalidOperationException($"Invalid window RuntimeId");

            if(_containers.TryGetValue(window.RunTimeId, out _))
            {
                LogEventFactory.RaiseText($"Window [{window.RunTimeId}] already has a container.");
                return;
            }

            CreateWindowContainer(window);
        }
    }

    public void NotifyWindowClosed(RunTimeId id, EventId eventId)
    {
        lock (_windowEventLock)
        {
            if (id.State != RunTimeIdStates.Valid)
            {
                LogEventFactory.RaiseText($"invalid window runtimeid");
                return;
            }

            if (_containers.TryGetValue(id, out _))
            {
                RemoveWindowContainer(id);
                return;
            }

            LogEventFactory.RaiseText($"Try to remove container, but it wasn't in collection");
        }
    }

    private WindowContainer ResolveContainer(BY by) => by.Scope switch
    {
        WindowScope.Default => _defaultContainer!,
        _ => throw new NotImplementedException()
    };

    private Order RegisterOrder(BY by)
    {
        var order = new Order { By = by };
        _orders.TryAdd(order.Id, order);
        
        return order;
    }

    private WindowContainer CreateWindowContainer(AutomationElementObject window)
    {
        if(window.RunTimeId.State != RunTimeIdStates.Valid)
            throw new Exception($"Invalid window RuntimeId: {string.Join(",", window.RunTimeId)}");

        var container = new WindowContainer(window, EventLibrary);
        container.RegisterOpenWindowEvent(_toggleWindowListener);
        container.RegisterCloseWindowEvent(_toggleWindowListener);
        if(!_containers.TryAdd(window.RunTimeId, container))
            throw new Exception($"Failed to add window container for window [{string.Join(",", window.RunTimeId)}].");

        LogEventFactory.RaiseText($"container wor window[{window.RunTimeId}] created.");
        return container;
    }

    private void RemoveWindowContainer(RunTimeId id)
    {
        if (_containers.TryRemove(id, out var container))
            container.Dispose();
        LogEventFactory.RaiseText("Window container removed: " + id);
    }
}
