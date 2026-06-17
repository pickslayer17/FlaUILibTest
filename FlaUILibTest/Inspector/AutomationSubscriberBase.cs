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
    private readonly Module _module;

    public AutomationSubscriberBase(Module module, ConditionBase condition)
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
            if (element != null && _pendingRequest != null)
            {
                _pendingRequest.TrySetResult(element);
            }
        }
    }

    protected async Task<AutomationElement> GetElement()
    {
        lock (_lock)
        {
            _pendingRequest = null;

            if (_cached != null && _module.State == ModuleState.Ready)
                return _cached;

            _pendingRequest = new TaskCompletionSource<AutomationElement>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        return await _pendingRequest.Task.WaitAsync(TimeSpan.FromMilliseconds(Timeout));
    }
}
