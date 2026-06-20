using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core.Identifiers;

namespace FlaUILibTest.Inspector;

public class WindowFinder : IDisposable
{
    public Action<WindowFinder, EventId, AutomationElement, string> OnWindowEvent;
    public Func<AutomationElement, ConditionBase, AutomationElement> SearchFunc { get; set; }

    private AutomationElement _root;
    public int[] RootRuntimeId { get; }
    public string Name { get; }
    private int _searchCount;
    
    private readonly Lock _watchesLock = new();
    private List<(ConditionBase condition, TaskCompletionSource<AutomationElement> tcs)> Watches
    {
        get
        {
            lock (_watchesLock)
            {
                return field;
            }
        }
        set
        {
            lock (_watchesLock)
            {
                if (field != null)
                {
                    foreach (var watch in field.Where(w => !w.tcs.Task.IsCompleted))
                        watch.tcs.TrySetCanceled();
                }
                field = value;
            }
        }
    } = new();
    private List<(ConditionBase condition, TaskCompletionSource<AutomationElement> tcs)> PendingWatches => Watches.Where(w => !w.tcs.Task.IsCompleted).ToList();

    private readonly Lock _searchLock = new();

    public AutomationElement DefaultSearch(AutomationElement root, ConditionBase condition)
    {
        lock (_searchLock)
        {
            var found = SearchFunc?.Invoke(root, condition);
            return found;
        }
    }

    public WindowFinder(Window window, Func<AutomationElement, ConditionBase, AutomationElement> searchFunc)
    {
        SearchFunc = searchFunc;
        RootRuntimeId = window.Properties.RuntimeId.ValueOrDefault;
        Name = window.Properties.Name.ValueOrDefault;
        Subscribe(window);
    }

    private void Subscribe(Window window)
    {
        Subscribe((AutomationElement)window);
        window.RegisterAutomationEvent(
            window.Automation.EventLibrary.Window.WindowOpenedEvent,
            TreeScope.Subtree,
            OnWindowOpened);
        window.RegisterAutomationEvent(
            window.Automation.EventLibrary.Window.WindowClosedEvent,
            TreeScope.Subtree,
            OnWindowClosed);
    }

    private void Subscribe(AutomationElement element)
    {
        _root = element;
        element.RegisterStructureChangedEvent(TreeScope.Subtree, OnStructureChanged);
        element.RegisterPropertyChangedEvent(TreeScope.Subtree, OnPropertyChanged, UiaPropertyHelper.AllProperties
            .Except(
                [
                    UiaPropertyHelper.GetPropertyId(UiaProperty.BoundingRectangle)
                ]
                ).ToArray());
    }

    public async Task<AutomationElement> RegisterAndGetElementAsync(ConditionBase condition, int timeoutMs = 15000)
    {
        Log($"RegisterAsync: searching from {GetElementInfo(_root)}");
        var found = DefaultSearch(_root, condition);
        if (found != null)
            return found;
        var tcs = new TaskCompletionSource<AutomationElement>(TaskCreationOptions.RunContinuationsAsynchronously);
        Watches.Add((condition, tcs));

        return await tcs.Task.WaitAsync(TimeSpan.FromMilliseconds(timeoutMs));
    }

    private void OnStructureChanged(AutomationElement element, StructureChangeType changeType, int[] runtimeId)
    {
        if (!(changeType == StructureChangeType.ChildAdded ||
            changeType == StructureChangeType.ChildrenInvalidated)) return;

        var info = GetElementInfo(element);
        //Log($"STRUCTURE ChildAdded | {info}");
        TryResolveByDescendant(element, info);
    }

    private void OnPropertyChanged(AutomationElement element, PropertyId propertyId, object newValue)
    {
        var info = GetElementInfo(element);
        //Log($"PROPERTY {propertyId.Name} = {newValue} | {info}");
        TryResolveByMatch(element, info);
    }

    private void OnWindowOpened(AutomationElement element, EventId eventId)
    {
        var info = GetElementInfo(element);
        Log($"WINDOW_OPENED | {info}");
        TryResolveByMatch(element, info);
        TryResolveByDescendant(element, info);
        OnWindowEvent?.Invoke(this, eventId, element, "windowOpened");
    }

    private void OnWindowClosed(AutomationElement element, EventId eventId)
    {
        var info = GetElementInfo(element);
        Log($"WINDOW_CLOSED | {info}");
        TryResolveByMatch(element, info);
        TryResolveByDescendant(element, info);
        OnWindowEvent?.Invoke(this, eventId, element, "windowClosed");
    }

    private void TryResolveByDescendant(AutomationElement element, string elementInfo)
    {
        foreach (var watch in PendingWatches)
        {
            var searchNum = Interlocked.Increment(ref _searchCount);
            try
            {
                var found = DefaultSearch(element, watch.condition);
                if (found != null)
                {
                    Log($">>> RESOLVED [descendant] #{searchNum} from {elementInfo}");
                    watch.tcs.TrySetResult(found);
                }
            }
            catch { }
        }
    }

    private void TryResolveByMatch(AutomationElement element, string elementInfo)
    {
        foreach (var watch in PendingWatches)
        {
            try
            {
                if (ConditionMatcher.Matches(element, watch.condition))
                {
                    Log($">>> RESOLVED [match] {elementInfo}");
                    watch.tcs.TrySetResult(element);
                }
            }
            catch { }
        }
    }

    private string GetElementInfo(AutomationElement element)
    {
        var name = "";
        var cls = "";
        var ct = "";
        var aid = "";
        try { ct = element.Properties.ControlType.ValueOrDefault.ToString(); } catch { }
        try { aid = element.Properties.AutomationId.ValueOrDefault; } catch { }
        try { name = element.Properties.Name.ValueOrDefault; } catch { }
        try { cls = element.Properties.ClassName.ValueOrDefault; } catch { }
        return $"Nm: '{name}' | Cls: '{cls}' | CnrtrlTp: '{ct}' | AutId: '{aid}'";
    }

    private void Log(string message)
    {
        LogManager.Log($"Finder '{Name}'", message);
    }

    public void Dispose()
    {
        Log("Disposing finder and cancelling pending watches[not implemented yet]");
    }
}