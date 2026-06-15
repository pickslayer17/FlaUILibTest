using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core.Identifiers;

namespace FlaUILibTest.Inspector;

public enum UiaProperty
{
    AutomationId = 30002,
    Name = 30005,
    IsEnabled = 30010,
    IsContentElement = 30012,
    IsControlElement = 30013,
    IsOffscreen = 30022,
    AriaProperties = 30043,
    ItemStatus = 30045
}

public static class UiaPropertyHelper 
{
    public static readonly PropertyId[] AllProperties = Enum.GetValues<UiaProperty>()
    .Select(p => PropertyId.Register(AutomationType.UIA3, (int)p, p.ToString()))
    .ToArray();
}

public class ModuleFinder
{
    private AutomationElement _root;
    private readonly string _name;
    private int _searchCount;

    private readonly object _watchesLock = new();
    private List<(ConditionBase condition, TaskCompletionSource<AutomationElement> tcs)> _watches;
    private List<(ConditionBase condition, TaskCompletionSource<AutomationElement> tcs)> Watches 
    {
        get 
        {
            lock (_watchesLock)
            {
                return _watches;
            }
        }
        set
        {
            lock (_watchesLock)
            {
                if (_watches != null)
                {
                    foreach (var watch in _watches.Where(w => !w.tcs.Task.IsCompleted))
                        watch.tcs.TrySetCanceled();
                }
                _watches = value;
            }
        }
    } 
    private List<(ConditionBase condition, TaskCompletionSource<AutomationElement> tcs)> PendingWatches => Watches.Where(w => !w.tcs.Task.IsCompleted).ToList();
    private void ResetWatches()
    {
        Watches = new();
    }

    public ModuleFinder(string name = "default")
    {
        _name = name;
    }

    public void Subscribe(Window window)
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

    public void Subscribe(AutomationElement element)
    {
        ResetWatches();
        _root = element;
        element.RegisterStructureChangedEvent(TreeScope.Subtree, OnStructureChanged);
        element.RegisterPropertyChangedEvent(TreeScope.Subtree, OnPropertyChanged, UiaPropertyHelper.AllProperties);
    }

    public async Task<AutomationElement> RegisterAndGetElementAsync(ConditionBase condition, int timeoutMs = 7000)
    {
        var found = DefaultSearch(condition);
        if (found != null)
            return found;
        var tcs = new TaskCompletionSource<AutomationElement>(TaskCreationOptions.RunContinuationsAsynchronously);
        Watches.Add((condition, tcs));

        return await tcs.Task.WaitAsync(TimeSpan.FromMilliseconds(timeoutMs));
    }
    
    private AutomationElement DefaultSearch(ConditionBase condition)
    {
        return _root.FindFirstDescendant(condition);
    }

    private void OnStructureChanged(AutomationElement element, StructureChangeType changeType, int[] runtimeId)
    {
        if (changeType != StructureChangeType.ChildAdded) return;
        var info = GetElementInfo(element);
        Log($"STRUCTURE ChildAdded | {info}");
        TryResolveByDescendant(element, info);
    }

    private void OnPropertyChanged(AutomationElement element, PropertyId propertyId, object newValue)
    {
        var info = GetElementInfo(element);
        Log($"PROPERTY {propertyId.Name} = {newValue} | {info}");
        TryResolveByMatch(element, info);
    }

    private void OnWindowOpened(AutomationElement element, EventId eventId)
    {
        var info = GetElementInfo(element);
        Log($"WINDOW_OPENED | {info}");
        TryResolveByMatch(element, info);
        TryResolveByDescendant(element, info);
    }

    private void OnWindowClosed(AutomationElement element, EventId eventId)
    {
        //stop work with this window
    }

    private void TryResolveByDescendant(AutomationElement element, string elementInfo)
    {
        foreach (var watch in PendingWatches)
        {
            var searchNum = Interlocked.Increment(ref _searchCount);
            try
            {
                var found = element.FindFirstDescendant(watch.condition);
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
        try { name = element.Properties.Name.ValueOrDefault; } catch { }
        try { cls = element.Properties.ClassName.ValueOrDefault; } catch { }
        return $"{name} | {cls}";
    }

    private void Log(string message)
    {
        Console.WriteLine($"[{_name}] {message}");
    }
}