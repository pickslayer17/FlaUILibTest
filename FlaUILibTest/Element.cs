using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUILibTest.Interfaces;

namespace FlaUILibTest;

public class Element : ISubscriber
{
    public static int Timeout { get; set; } = 10_000;

    private int SelfTimeout { get; set; } = 0;
    private readonly Module _module;
    private AutomationElement? _cached;
    private TaskCompletionSource<AutomationElement>? _waiter;
    private readonly object _lock = new();

    public ConditionBase SelfCondition { get; }

    public Element(Module module, ConditionBase condition)
    {
        _module = module;
        SelfCondition = condition;
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

    public Task<string> GetNameAsync() => WithElement(el => el.Properties.Name.ValueOrDefault ?? "");
    public Task<string> GetValueAsync() => WithElement(el => el.Patterns.Value.Pattern.Value?.ToString());
    public Task ClickAsync() => WithElement(el => el.Click());

    private async Task<T> WithElement<T>(Func<AutomationElement, T> action)
    {
        var el = await GetElement();
        return action(el);
    }

    private async Task WithElement(Action<AutomationElement> action)
    {
        var el = await GetElement();
        action(el);
    }

    private async Task<AutomationElement> GetElement()
    {
        lock (_lock)
        {
            if (_module.State == ModuleState.NotInitialized)
                _module.TryInitialize();
            if (_cached != null && _module.State == ModuleState.Ready)
                return _cached;

            _waiter ??= new TaskCompletionSource<AutomationElement>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        return await _waiter.Task;
    }
}