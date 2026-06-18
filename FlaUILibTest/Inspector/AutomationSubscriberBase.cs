using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;

namespace FlaUILibTest.Inspector;

public abstract class AutomationSubscriberBase
{
    private AutomationElement? _cached;
    private readonly object _lock = new();

    public ConditionBase SelfCondition { get; init; }
    private ModuleFinder _moduleFinder;

    public AutomationSubscriberBase(ModuleFinder moduleFinder, ConditionBase condition)
    {
        SelfCondition = condition;
        _moduleFinder = moduleFinder;
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
            if (_cached != null)
                return _cached;
        }

        _cached = await _moduleFinder.RegisterAndGetElementAsync(SelfCondition);

        return _cached;
    }
}
