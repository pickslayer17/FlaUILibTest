using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;

namespace FlaUILibTest.Inspector;

public class EventManager
{
    private static EventManager? _instance;
    private static readonly object _instanceLock = new();

    private readonly List<Module> _modules = new();
    private readonly object _modulesLock = new();

    private EventManager() { }

    public static EventManager Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_instanceLock)
                {
                    _instance ??= new EventManager();
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
}
