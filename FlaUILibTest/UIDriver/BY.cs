using FlaUI.Core.Conditions;

namespace FlaUILibTest.UIDriver;

public class BY
{
    public ConditionBase Condition { get; }
    public BY(ConditionBase condition)
    {
        Condition = condition;
    }
}
