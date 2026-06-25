using System.Collections.Concurrent;

namespace UIDriver;

public sealed class ApplicationManager
{
    private readonly ConcurrentDictionary<RunTimeId, WindowContainer> _containers = new();
    private readonly ConcurrentDictionary<Guid, Order> _orders = new();
    private readonly ToggleWindowListener _toggleWindowListener;

    private WindowContainer? _defaultContainer;
    private WindowContainer? _desktopContainer;

    public ApplicationManager() => _toggleWindowListener = new ToggleWindowListener(this);

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

    public WindowContainer CreateWindowContainer(AutomationElementObject window)
    {
        var container = new WindowContainer(window);
        container.RegisterOpenWindowEvent(_toggleWindowListener);
        container.RegisterCloseWindowEvent(_toggleWindowListener);
        _containers.TryAdd(window.RunTimeId, container);
        LogEventFactory.RaiseWindowEventBase(window.RunTimeId);
        return container;
    }

    public void RemoveWindowContainer(RunTimeId id)
    {
        if (_containers.TryRemove(id, out var container))
            container.Dispose();
        LogEventFactory.RaiseText("Window container removed: " + id);
    }
}
