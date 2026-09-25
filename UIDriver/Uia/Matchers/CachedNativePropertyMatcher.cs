using Interop.UIAutomationClient;

public sealed class CachedNativePropertyMatcher
{
    private readonly IUIAutomationCondition _condition;

    public CachedNativePropertyMatcher(IUIAutomationCondition condition)
    {
        _condition = condition;
    }

    public bool Matches(IUIAutomationElement element) => Matches(element, _condition);

    private bool Matches(IUIAutomationElement element, IUIAutomationCondition condition) => condition switch
    {
        IUIAutomationPropertyCondition propertyCondition => PropertyMatches(element, propertyCondition),
        IUIAutomationAndCondition andCondition => GetChildren(andCondition.GetChildren()).All(child => Matches(element, child)),
        IUIAutomationOrCondition orCondition => GetChildren(orCondition.GetChildren()).Any(child => Matches(element, child)),
        IUIAutomationNotCondition notCondition => !Matches(element, (IUIAutomationCondition)notCondition.GetChild()),
        IUIAutomationBoolCondition boolCondition => boolCondition.BooleanValue != 0,
        _ => throw new NotImplementedException($"Condition type {condition.GetType().Name} is not supported."),
    };

    private bool PropertyMatches(IUIAutomationElement element, IUIAutomationPropertyCondition propertyCondition)
    {
        var actual = GetCachedPropertyValue(element, propertyCondition.propertyId);
        var expected = propertyCondition.PropertyValue;
        return Equals(actual, expected);
    }

    private static IEnumerable<IUIAutomationCondition> GetChildren(Array children)
        => children.Cast<IUIAutomationCondition>();

    private object? GetCachedPropertyValue(IUIAutomationElement element, int propertyId)
    {
        try { return element.GetCachedPropertyValue(propertyId); }
        catch { return null; }
    }
}
