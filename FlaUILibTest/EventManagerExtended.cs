using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Identifiers;

namespace FlaUILibTest;

public class EventManagerExtended
{
    private static EventManagerExtended? _instance;
    private static readonly object _instanceLock = new();

    private readonly List<Module> _modules = new();
    private readonly object _modulesLock = new();

    private EventManagerExtended() { }

    public static EventManagerExtended Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_instanceLock)
                {
                    _instance ??= new EventManagerExtended();
                }
            }
            return _instance;
        }
    }

    public void Subscribe(Window window)
    {
        window.RegisterStructureChangedEvent(
            TreeScope.Subtree,
            OnStructureChanged);
    }

    public void Register(Module module)
    {
        lock (_modulesLock)
        {
            _modules.Add(module);
        }
    }

    public void Unregister(Module module)
    {
        lock (_modulesLock)
        {
            _modules.Remove(module);
        }
    }

    private void OnStructureChanged(AutomationElement element, StructureChangeType changeType, int[] runtimeId)
    {
        List<Module> snapshot;
        lock (_modulesLock)
        {
            snapshot = new List<Module>(_modules);
        }

        foreach (var module in snapshot)
        {
            if (module.MatchesEvent(element, changeType, runtimeId))
            {
                module.Notify(element, changeType);
            }
        }
    }












    public void SubscribeAll(Window window)
    {
        window.RegisterStructureChangedEvent(TreeScope.Subtree, OnStructureChangedExtended);

        window.RegisterAutomationEvent(
            window.Automation.EventLibrary.Window.WindowOpenedEvent,
            TreeScope.Subtree,
            OnAutomationEvent);

        window.RegisterPropertyChangedEvent(
            TreeScope.Subtree,
            OnPropertyChanged,
            PropertyId.Register(AutomationType.UIA3, 30003, "Name"),           // Name
            PropertyId.Register(AutomationType.UIA3, 30010, "IsEnabled"),      // IsEnabled
            PropertyId.Register(AutomationType.UIA3, 30005, "BoundingRectangle") // BoundingRectangle
        );

        window.RegisterAutomationEvent(
            window.Automation.EventLibrary.SelectionItem.ElementSelectedEvent,
            TreeScope.Subtree,
            OnAutomationEvent);

        window.RegisterAutomationEvent(
            window.Automation.EventLibrary.Invoke.InvokedEvent,
            TreeScope.Subtree,
            OnAutomationEvent);
    }

    private void OnStructureChangedExtended(AutomationElement element, StructureChangeType changeType, int[] runtimeId)
    {
        var name = TryGet(() => element.Properties.Name.ValueOrDefault);
        var cls = TryGet(() => element.Properties.ClassName.ValueOrDefault);
        var ctrl = TryGet(() => element.Properties.ControlType.ValueOrDefault.ToString());

        LogEvent("STRUCTURE",
            $"type  : {changeType}",
            $"name  : {name}",
            $"class : {cls}",
            $"ctrl  : {ctrl}");
    }

    private void OnAutomationEvent(AutomationElement element, EventId eventId)
    {
        var name = TryGet(() => element.Properties.Name.ValueOrDefault);
        var cls = TryGet(() => element.Properties.ClassName.ValueOrDefault);

        LogEvent("AUTOMATION",
            $"event : {eventId.Name}",
            $"name  : {name}",
            $"class : {cls}");
    }

    private void OnPropertyChanged(AutomationElement element, PropertyId propertyId, object newValue)
    {
        var name = TryGet(() => element.Properties.Name.ValueOrDefault);
        var cls = TryGet(() => element.Properties.ClassName.ValueOrDefault);

        LogEvent("PROPERTY",
            $"prop  : {propertyId.Name}",
            $"value : {newValue}",
            $"name  : {name}",
            $"class : {cls}");
    }

    private static string TryGet(Func<string> getter)
    {
        try { return getter() ?? ""; }
        catch { return "<dead>"; }
    }

    private static readonly object _logLock = new();
    private static void LogEvent(string category, params string[] lines)
    {
        lock (_logLock)
        {
            Console.WriteLine($"--- {category} ---");
            foreach (var line in lines)
                Console.WriteLine($"  {line}");
        }
    }
}
