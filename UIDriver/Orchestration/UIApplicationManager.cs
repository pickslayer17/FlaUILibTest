using FlaUI.Core;
using FlaUI.Core.Identifiers;
using System.Collections.Concurrent;
using UIDriver.Constants;
using UIDriver.CustomModels;

namespace UIDriver;

public sealed class UIApplicationManager
{
    public int ProcessId { get; set; }

    private readonly ConcurrentDictionary<RunTimeId, WindowContainer> _containers = new();
    private readonly ConcurrentDictionary<Guid, Order> _orders = new();
    private readonly ToggleWindowListener _toggleWindowListener;

    private Lock _windowEventLock = new();

    private WindowContainer? _defaultContainer;
    private WindowContainer? _desktopContainer;

    public UIApplicationManager()
    {
        _toggleWindowListener = new ToggleWindowListener(this);
    }

    public void RegisterDefault(UIAutomationElement window) => _defaultContainer = CreateWindowContainer(window);

    public void RegisterDesktop(UIAutomationElement window) => _desktopContainer = CreateWindowContainer(window);

    public Task<UIAutomationElement> RequestElementAsync(UIBy by)
    {
        lock (_windowEventLock)
        {
            var container = ResolveContainer(by);
            var order = RegisterOrder(by);

            var task = container.SubmitOrderAsync(order);
            order.Task = task;
            return task;
        }
    }

    // Lock method. all changing of _containers are inside these 2 methods. Dont want to handle it right now. but i think non-locked code should be separated from lock-mechanism in the future
    public void NotifyWindowOpened(UIAutomationElement window, EventId eventId)
    {
        lock (_windowEventLock)
        {
            if (window.RunTimeId.State != RunTimeIdStates.Valid)
                throw new InvalidOperationException($"Invalid window RuntimeId");

            if(_containers.TryGetValue(window.RunTimeId, out _))
            {
                LogEventFactory.RaiseText($"Window [{window.RunTimeId}] already has a container.");
                return;
            }

            CreateWindowContainer(window);
            LogContainers();
        }
    }

    // Lock method
    public void NotifyWindowClosed(RunTimeId id, EventId eventId)
    {
        lock (_windowEventLock)
        {
            if (id.State != RunTimeIdStates.Valid)
            {
                throw new InvalidOperationException("should be always valid. smth went wrong");
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

    private void LogContainers()
    {
        LogEventFactory.RaiseText($"\ncontainers count = {_containers.Count}");
        foreach (var (i, kvp) in _containers.Select((kvp, i) => (i, kvp)))
        {
            LogEventFactory.RaiseText($"[CONTAINER][{i}] = [{kvp.Key}][{kvp.Value.WindowTitle}]");
        }
        LogEventFactory.RaiseText($"\n\n");
    }

    private WindowContainer ResolveContainer(UIBy by) => by.Scope switch
    {
        WindowScope.Default => _defaultContainer!,
        WindowScope.Desktop => _desktopContainer!,
        _ => throw new NotImplementedException()
    };

    private Order RegisterOrder(UIBy by)
    {
        var order = new Order { By = by };
        _orders.TryAdd(order.Id, order);
        
        return order;
    }

    private void ReassignDefaultContainer()
    {
        var allApplicationContainers = _containers.Where(kv => kv.Value != _desktopContainer).Where(kv => kv.Value.ProcessId == ProcessId);
        if (!allApplicationContainers.Any())
        {
            LogEventFactory.RaiseText($"IT seems there is no target process window anymore");
            throw new NotImplementedException();//should be some logic, dont know which
            return;
        }

        _defaultContainer = allApplicationContainers.First().Value;
        LogEventFactory.RaiseText($"Default container reassigned");
    }

    private bool IsDefaultContainerExists() => _containers.Any(kvp => ReferenceEquals(kvp.Value, _defaultContainer));

    private WindowContainer CreateWindowContainer(UIAutomationElement window)
    {
        if(window.RunTimeId.State != RunTimeIdStates.Valid)
            throw new Exception($"Invalid window RuntimeId: {string.Join(",", window.RunTimeId)}");

        var container = new WindowContainer(window);
        container.RegisterToggleWindowEvent(_toggleWindowListener);
        if(!_containers.TryAdd(window.RunTimeId, container))
            throw new Exception($"Failed to add window container for window [{string.Join(",", window.RunTimeId)}].");

        LogContainers();
        return container;
    }

    private void RemoveWindowContainer(RunTimeId id)
    {
        if (_containers.TryRemove(id, out var container))
        {
            container.Dispose();

            if (!IsDefaultContainerExists())
            {
                ReassignDefaultContainer();
            }
          
            LogContainers();
        }
        else
        {
            throw new InvalidProgramException("we have check on container exist, so its very strange that is wasnt removed");
        }
    }
}
