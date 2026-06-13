using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUILibTest;

namespace FlaUILibTest;

public class Element
{
    private readonly Module _module;
    private readonly ConditionBase _condition;
    private AutomationElement? _cached;
    private TaskCompletionSource<AutomationElement>? _waiter;
    private readonly object _lock = new();

    public ConditionBase Condition => _condition;

    public Element(Module module, ConditionBase condition)
    {
        _module = module;
        _condition = condition;
        _module.AddSubscriber(this);
    }

    public void Update(AutomationElement? element)
    {
        lock (_lock)
        {
            _cached = element;

            if (element != null && _waiter != null)
            {
                _waiter.TrySetResult(element);
                _waiter = null;
            }
        }
    }

    public async Task<AutomationElement> GetElement()
    {
        lock (_lock)
        {
            if (_cached != null && _module.State == ModuleState.Ready)
                return _cached;

            _waiter ??= new TaskCompletionSource<AutomationElement>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        return await _waiter.Task;
    }

    public async Task Click()
    {
        var el = await GetElement();
        el.Click();
    }

    public async Task<string> GetName()
    {
        var el = await GetElement();
        return el.Properties.Name.ValueOrDefault ?? "";
    }

    public async Task<string> GetValue()
    {
        var el = await GetElement();
        return el.Patterns.Value.Pattern.Value;
    }
}