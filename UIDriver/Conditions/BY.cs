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

    public ConditionBase? Element { get; init; }

    public BY? Parent { get; init; }

    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(15);
}
