using FlaUI.Core.Conditions;

namespace UIDriver;

public enum WindowScope
{
    Desktop,
    Default,
    Custom
}

public sealed class BY
{
    public WindowScope Scope { get; init; } = WindowScope.Default;

    public ConditionBase? SelfCondition { get; init; }

    public BY? Parent { get; init; }

    public BY[]? Children { get; init; }

    public BY[]? Ancestors { get; init; }

    public BY[]? Descendants { get; init; }

    public BY[]? Siblings { get; init; }

    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(15);
}
