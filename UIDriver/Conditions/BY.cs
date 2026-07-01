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

    public ConditionBase? Root { get; init; }
    public BY? AncestorOrParent { get; init; }
    public bool IsChild { get; init; }
    public BY[]? PrecedingSiblings { get; init; }
    public BY[]? Preceding { get; init; }
    public ConditionBase? SelfCondition { get; init; }
    public BY[]? FollowingSiblings { get; init; }
    public BY[]? Following { get; init; }
    public BY[]? Children { get; init; }
    public BY[]? Descendants { get; init; }
    public int? SiblingIndex { get; init; }

    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(15);
}
