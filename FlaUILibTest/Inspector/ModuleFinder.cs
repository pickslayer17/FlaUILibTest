using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core.Identifiers;

namespace FlaUILibTest.Inspector;

public class ModuleFinder
{
    private AutomationElement _root;
    private readonly string _name;
    private int _searchCount;

    private readonly object _watchesLock = new();
    private List<(ConditionBase condition, TaskCompletionSource<AutomationElement> tcs)> _watches = new();
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

    private ModuleFinder() { }

    public ModuleFinder(Window element, string name = "default")
    {
        _name = name;
        Subscribe(element);
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
        element.RegisterPropertyChangedEvent(TreeScope.Subtree, OnPropertyChanged, UiaPropertyHelper.TestProperties);
    }

    public async Task<AutomationElement> RegisterAndGetElementAsync(ConditionBase condition, int timeoutMs = 7000)
    {
        Log($"RegisterAsync: searching from {GetElementInfo(_root)}");
        var found = DefaultSearch(condition);
        if (found != null)
            return found;
        var tcs = new TaskCompletionSource<AutomationElement>(TaskCreationOptions.RunContinuationsAsynchronously);
        Watches.Add((condition, tcs));

        return await tcs.Task.WaitAsync(TimeSpan.FromMilliseconds(timeoutMs));
    }

    private AutomationElement DefaultSearch(ConditionBase condition)
    {
        try { return _root.FindFirstDescendant(condition); }
        catch (Exception ex)
        {
            Log($"[!!!ERROR!!!] DefaultSearch failed: {ex.Message}");
            return null;
        }
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
        Console.WriteLine($"[{_name}] {message}");
    }
}