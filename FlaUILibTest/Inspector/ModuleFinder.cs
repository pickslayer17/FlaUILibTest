using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core.Identifiers;
using System.Reflection;

namespace FlaUILibTest.Inspector;

public class ModuleFinder
{
    private Module _module;


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

    private readonly object _searchLock = new();

    public AutomationElement DefaultSearch(AutomationElement root, ConditionBase condition)
    {
        lock (_searchLock)
        {
            return root.FindFirstDescendant(condition);
        }
    }

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
        element.RegisterPropertyChangedEvent(TreeScope.Subtree, OnPropertyChanged, UiaPropertyHelper.AllProperties
            .Except(
                [
                    UiaPropertyHelper.GetPropertyId(UiaProperty.BoundingRectangle)
                ]
                ).ToArray()); //TestProperties);
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

        if (_module?.Self != null)
        {
            try
            {
                var moduleRid = _module.Self.Properties.RuntimeId.ValueOrDefault;
                var eventRid = element.Properties.RuntimeId.ValueOrDefault;
                if (moduleRid != null && eventRid != null && moduleRid.SequenceEqual(eventRid))
                    _module.Notify(changeType);
            }
            catch { }
        }
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
    }

    private void OnWindowClosed(AutomationElement element, EventId eventId)
    {
        var info = GetElementInfo(element);
        Log($"WINDOW_CLOSED | {info}");
        //TryResolveByMatch(element, info);
        //TryResolveByDescendant(element, info);
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

                    _module = new Module(this, element);
                    _module.AddSubscriber(new Element(this, watch.condition));

                    Log(">>> module created");
                    
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