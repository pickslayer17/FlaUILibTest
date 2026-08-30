using Interop.UIAutomationClient;
using System.Collections.Concurrent;
using UIDriver.Constants;
using UIDriver.CustomModels;
using UIDriver.Visualization;

namespace UIDriver;

public sealed class UIApplicationManager
{
    public int ProcessId { get; set; }

    private readonly IUIAutomation _automation;
    private readonly ITreeSnapshotSink _snapshotSink;
    private readonly ConcurrentDictionary<RunTimeId, WindowContainer> _containers = new();
    private readonly ToggleWindowListener _toggleWindowListener;

    private Lock _windowEventLock = new();

    private WindowContainer? _defaultContainer;
    private WindowContainer? _desktopContainer;

    public UIApplicationManager(IUIAutomation automation)
        : this(automation, Visualization.TreeVisualizer.Instance)
    {
    }

    public UIApplicationManager(IUIAutomation automation, ITreeSnapshotSink snapshotSink)
    {
        _automation = automation;
        _snapshotSink = snapshotSink;
        _toggleWindowListener = new ToggleWindowListener(this);
    }

    public void RegisterDefault(IUIAutomationElement window) => _defaultContainer = CreateWindowContainer(window);

    public void RegisterDesktop(IUIAutomationElement window) => _desktopContainer = CreateWindowContainer(window);

    public Task<IUIAutomationElement> RequestElementAsync(UIBy by)
    {
        lock (_windowEventLock)
        {
            return _defaultContainer!.SubmitOrderAsync(by);
        }
    }

    public void NotifyWindowOpened(IUIAutomationElement window)
    {
        lock (_windowEventLock)
        {
            var windowRunTimeId = window.LiveRuntimeId();

            if (windowRunTimeId.State != RunTimeIdStates.Valid)
                throw new InvalidOperationException($"Invalid window RuntimeId");

            if(_containers.TryGetValue(windowRunTimeId, out _))
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

    public void PrintCollectedTreesParents()
    {
        foreach (var container in _containers.Values)
            container.CacheTreeManager.PrintCollectedTreesParents();
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

    private WindowContainer CreateWindowContainer(IUIAutomationElement window)
    {
        var windowRunTimeId = window.LiveRuntimeId();

        if(windowRunTimeId.State != RunTimeIdStates.Valid)
            throw new Exception($"Invalid window RuntimeId: {windowRunTimeId}");

        var container = new WindowContainer(window, _automation, _snapshotSink);
        container.RegisterToggleWindowEvent(_toggleWindowListener);
        if(!_containers.TryAdd(windowRunTimeId, container))
            throw new Exception($"Failed to add window container for window [{windowRunTimeId}].");

        container.PublishInitialSnapshot();
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
        }
        else
        {
            throw new InvalidProgramException("we have check on container exist, so its very strange that is wasnt removed");
        }
    }
}
