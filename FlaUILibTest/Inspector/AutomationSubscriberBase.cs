using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;

namespace FlaUILibTest.Inspector;

public abstract class AutomationSubscriberBase
{
    private const int Timeout = 5000;
    private AutomationElement? _cached;
    private TaskCompletionSource<AutomationElement>? _pendingRequest;
    private readonly object _lock = new();

    public ConditionBase SelfCondition { get; init; }
    private Module _module;
    private ModuleFinder _moduleFinder;

    public AutomationSubscriberBase(ModuleFinder moduleFinder, ConditionBase condition)
    {
        SelfCondition = condition;
        _moduleFinder = moduleFinder;
    }

    public void SetModule(Module module)
    {
        _module = module;
    }

    public void Update(AutomationElement? element)
    {
        lock (_lock)
        {
            _cached = element;
        }
    }

    protected async Task<AutomationElement> GetElement()
    {
        lock (_lock)
        {
            if (_cached != null && (_module == null || _module.State == ModuleState.Ready))
                return _cached;
        }

        _cached = await _moduleFinder.RegisterAndGetElementAsync(SelfCondition);

        return _cached;
    }
}
