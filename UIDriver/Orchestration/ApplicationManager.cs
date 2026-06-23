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

    public async Task<AutomationElementObject> Request(BY by)
    {
        var container = by.Scope switch
        {
            WindowScope.Default => _defaultContainer!,
            _ => throw new NotImplementedException()
        };

        var order = new Order { By = by };
        _orders.TryAdd(Guid.NewGuid(), order);
        order.Task = container.Accept(order);
        return await order.Task;
    }

    public WindowContainer CreateWindowContainer(AutomationElementObject window)
    {
        var container = new WindowContainer(window);
        container.RegisterOpenWindowEvent(_toggleWindowListener);
        container.RegisterCloseWindowEvent(_toggleWindowListener);
        _containers.TryAdd(window.RunTimeId, container);
        return container;
    }

    public void RemoveWindowContainer(RunTimeId id)
    {
        if (_containers.TryRemove(id, out var container))
            container.Dispose();
    }
}
