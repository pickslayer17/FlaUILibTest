using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Identifiers;

public sealed class PropertyMatcher
{
    private readonly ConditionBase _condition;

    public PropertyMatcher(ConditionBase condition)
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
        var actual = GetPropertyValue(element, propertyCondition.Property);
        return EqualityComparer<object?>.Default.Equals(actual, propertyCondition.Value);
    }

    private object? GetPropertyValue(AutomationElement element, PropertyId propertyId)
    {
        try { return element.FrameworkAutomationElement.GetPropertyValue(propertyId); }
        catch { return null; }
    }
}
