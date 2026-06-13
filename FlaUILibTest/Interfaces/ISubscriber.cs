using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;

namespace FlaUILibTest.Interfaces;

public interface ISubscriber
{
    public ConditionBase SelfCondition { get; }
    public void Update(AutomationElement? element);
}

