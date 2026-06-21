using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core.EventHandlers;
using FlaUI.Core.Identifiers;
using FlaUILibTest.Extensions;
using FlaUILibTest.Helpers;
using FlaUILibTest.UIDriver;

namespace FlaUILibTest.Inspector;

public abstract class FinderBase : IDisposable
{
    public int TimeOut = 15_000;

    public Action<FinderBase, AutomationElement, EventId, int[]> OnWindowOpenedFunc;
    public Action<FinderBase, AutomationElement, EventId, int[]> OnWindowClosedFunc;
    public Func<AutomationElement, ConditionBase, AutomationElement> SearchFunc { get; set; }

    private StructureChangedEventHandlerBase StructureChangedHandler { get; set; }
    private PropertyChangedEventHandlerBase PropertyChangedHandler { get; set; }
    private AutomationEventHandlerBase WindowOpenedEventHandler { get; set; }
    private AutomationEventHandlerBase WindowClosedEventHandler { get; set; }

    public AutomationElement Window { get; init; }
    public int[] RootRuntimeId { get; }
    public string Name { get; }
    private int _searchCount;

    private readonly Lock _watchesLock = new();
    private readonly Lock _searchLock = new();

    private List<(ConditionBase condition, TaskCompletionSource<AutomationElement> tcs)> Watches
    {
        get
        {
            return field;
        }
        set
        {

            if (field != null)
            {
                foreach (var watch in field.Where(w => !w.tcs.Task.IsCompleted))
                    watch.tcs.TrySetCanceled();
            }
            field = value;
        }
    } = new();

    private List<(ConditionBase condition, TaskCompletionSource<AutomationElement> tcs)> PendingWatches
    {
        get
        {
            lock (_watchesLock)
            {
                return Watches.Where(w => !w.tcs.Task.IsCompleted).ToList();
            }
        }
    }

    public FinderBase(AutomationElement window)
    {
        RootRuntimeId = window.Properties.RuntimeId.ValueOrDefault;
        Name = window.Properties.Name.ValueOrDefault;
        Window = window;
    }

    public AutomationElement DefaultSearch(AutomationElement root, ConditionBase condition)
    {
        lock (_searchLock)
        {
            var found = SearchFunc?.Invoke(root, condition);
            return found;
        }
    }

    public void StartListening()
    {
        Subscribe();
    }

    public async Task<AutomationElement> RegisterAndGetElementAsync(BY condition)
    {
        Log($"RegisterAsync: searching from {GetElementInfo(Window)}");
        var tcs = new TaskCompletionSource<AutomationElement>(TaskCreationOptions.RunContinuationsAsynchronously);
        lock (_watchesLock)
        {
            Watches.Add((condition.Condition, tcs));
        }

        if (!tcs.Task.IsCompleted)
        {
            var found = DefaultSearch(Window, condition.Condition);
            if (found != null)
            {
                return found;
            }
        }

        try
        {
            return await tcs.Task.WaitAsync(TimeSpan.FromMilliseconds(TimeOut));
        }
        catch (TimeoutException)
        {
            Log($"RegisterAsync TIMEOUT after {TimeOut}ms for condition: {condition}");
            tcs.TrySetCanceled();
            throw;
        }
    }

    private void Subscribe()
    {
        StructureChangedHandler = Window.RegisterStructureChangedEvent(TreeScope.Subtree, OnStructureChanged);
        PropertyChangedHandler = Window.RegisterPropertyChangedEvent(TreeScope.Subtree, OnPropertyChanged, UiaPropertyHelper.AllProperties
            .Except(
                [
                    UiaPropertyHelper.GetPropertyId(UiaProperty.BoundingRectangle)
                ]
                ).ToArray());
        WindowOpenedEventHandler = Window.RegisterAutomationEvent(
            Window.Automation.EventLibrary.Window.WindowOpenedEvent,
            TreeScope.Subtree,
            OnWindowOpened);
        WindowClosedEventHandler = Window.RegisterAutomationEvent(
            Window.Automation.EventLibrary.Window.WindowClosedEvent,
            TreeScope.Element,
            OnWindowClosed);
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
        if (element.AsWindow().TryGetWindowRunTimeId(out int[] windowRunTimeId))
        {
            OnWindowOpenedFunc?.Invoke(this, element, eventId, windowRunTimeId);
        }
        else
        {
            LogManager.LogError("Failed to get RuntimeId");
        }
    }

    private void OnWindowClosed(AutomationElement element, EventId eventId)
    {
        var info = GetElementInfo(element);
        Log($"WINDOW_CLOSED | {info}");
        OnWindowClosedFunc?.Invoke(this, element, eventId, RootRuntimeId);
    }

    private void TryResolveByDescendant(AutomationElement element, string elementInfo)
    {
        foreach (var watch in PendingWatches)
        {
            var searchNum = Interlocked.Increment(ref _searchCount);
            var found = DefaultSearch(element, watch.condition);
            if (found != null)
            {
                Log($">>> RESOLVED [descendant] #{searchNum} from {elementInfo}");
                watch.tcs.TrySetResult(found);
            }

        }
    }

    private void TryResolveByMatch(AutomationElement element, string elementInfo)
    {
        foreach (var watch in PendingWatches)
        {
            if (ConditionMatcher.Matches(element, watch.condition))
            {
                Log($">>> RESOLVED [match] {elementInfo}");
                watch.tcs.TrySetResult(element);
            }
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
        LogManager.Log($"{Name} - {message}");
    }

    public void Dispose()
    {
        Log("Disposing finder and cancelling pending watches[not implemented yet]");
    }
}

