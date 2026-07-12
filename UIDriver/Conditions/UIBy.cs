using FlaUI.Core.Conditions;

namespace UIDriver;

public enum WindowScope
{
    Desktop,
    Default,
    Custom
}

public sealed class UIBy
{
    public WindowScope Scope { get; init; } = WindowScope.Default;

    public ConditionBase? Root { get; init; }
    public UIBy? AncestorOrParent { get; init; }
    public bool IsChild { get; init; }
    public UIBy[]? PrecedingSiblings { get; init; }
    public UIBy[]? Preceding { get; init; }
    public ConditionBase? SelfCondition { get; init; }
    public UIBy[]? FollowingSiblings { get; init; }
    public UIBy[]? Following { get; init; }
    public UIBy[]? Children { get; init; }
    public UIBy[]? Descendants { get; init; }
    public int? SiblingIndex { get; init; }

    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(15);
}
