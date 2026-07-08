using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Identifiers;

public sealed class CachedPropertyMatcher
{
    private static readonly System.Reflection.MethodInfo InternalGetPropertyValue =
        typeof(FlaUI.Core.FrameworkAutomationElementBase).GetMethod(
            "InternalGetPropertyValue",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
            null,
            new[] { typeof(int), typeof(bool), typeof(bool) },
            null)!;

    private readonly ConditionBase _condition;

    public CachedPropertyMatcher(ConditionBase condition)
    {
        _condition = condition;
    }

    public bool Matches(AutomationElement element) => Matches(element, _condition);

    private bool Matches(AutomationElement element, ConditionBase condition) => condition switch
    {
        PropertyCondition propertyCondition => PropertyMatches(element, propertyCondition),
        AndCondition andCondition => andCondition.Conditions.All(child => Matches(element, child)),
        OrCondition orCondition => orCondition.Conditions.Any(child => Matches(element, child)),
        NotCondition notCondition => !Matches(element, notCondition.Condition),
        TrueCondition => true,
        FalseCondition => false,
        _ => throw new NotImplementedException($"Condition type {condition.GetType().Name} is not supported."),
    };

    private bool PropertyMatches(AutomationElement element, PropertyCondition propertyCondition)
    {
        var actual = GetCachedPropertyValue(element, propertyCondition.Property);
        return EqualityComparer<object?>.Default.Equals(actual, propertyCondition.Value);
    }

    private object? GetCachedPropertyValue(AutomationElement element, PropertyId propertyId)
    {
        try { return InternalGetPropertyValue.Invoke(element.FrameworkAutomationElement, new object[] { propertyId.Id, true, false }); }
        catch { return null; }
    }
}
