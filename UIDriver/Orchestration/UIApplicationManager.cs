using Interop.UIAutomationClient;
using System.Collections.Concurrent;
using UIDriver.Constants;
using UIDriver.CustomModels;

namespace UIDriver;

public sealed class UIApplicationManager
{
    public int ProcessId { get; set; }

    private readonly IUIAutomation _automation;
    private readonly ConcurrentDictionary<RunTimeId, WindowContainer> _containers = new();
    private readonly ToggleWindowListener _toggleWindowListener;

    private Lock _windowEventLock = new();

    private WindowContainer? _defaultContainer;
    private WindowContainer? _desktopContainer;

    public UIApplicationManager(IUIAutomation automation)
    {
        _automation = automation;
        _toggleWindowListener = new ToggleWindowListener(this);
    }

    public void RegisterDefault(UIAutomationElement window) => _defaultContainer = CreateWindowContainer(window);

    public void RegisterDesktop(UIAutomationElement window) => _desktopContainer = CreateWindowContainer(window);

    public Task<UIAutomationElement> RequestElementAsync(UIBy by)
    {
        lock (_windowEventLock)
        {
            return _defaultContainer!.SubmitOrderAsync(by);
        }
    }

    public void NotifyWindowOpened(UIAutomationElement window)
    {
        lock (_windowEventLock)
        {
            if (window.RunTimeId.State != RunTimeIdStates.Valid)
                throw new InvalidOperationException($"Invalid window RuntimeId");

            if(_containers.TryGetValue(window.RunTimeId, out _))
            {
                return;
            }

            CreateWindowContainer(window);
        }
    }

    public void NotifyWindowClosed(RunTimeId id)
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

        }
    }

    private void LogContainers()
    {
        var snapshot = _containers.Values
            .Select(container => (container.WindowTitle, container.CacheTreeManager.Tree))
            .ToList();

        Visualization.TreeVisualizer.Render(snapshot);
    }

    private void ReassignDefaultContainer()
    {
        var allApplicationContainers = _containers.Where(kv => kv.Value != _desktopContainer).Where(kv => kv.Value.ProcessId == ProcessId);
        if (!allApplicationContainers.Any())
        {
            throw new NotImplementedException();//should be some logic, dont know which
        }

        _defaultContainer = allApplicationContainers.First().Value;
    }

    private bool IsDefaultContainerExists() => _containers.Any(kvp => ReferenceEquals(kvp.Value, _defaultContainer));

    private WindowContainer CreateWindowContainer(UIAutomationElement window)
    {
        if(window.RunTimeId.State != RunTimeIdStates.Valid)
            throw new Exception($"Invalid window RuntimeId: {string.Join(",", window.ToString())}");

        var container = new WindowContainer(window, _automation);
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
