using FlaUI.Core.Conditions;
using FlaUI.Core.Identifiers;

namespace FlaUILibTest;

public sealed class UiNodePropMatcher
{
    private readonly ConditionBase _condition;

    public UiNodePropMatcher(ConditionBase condition)
    {
        _condition = condition;
    }

    public bool Matches(UiNode element) => Matches(element, _condition);

    private bool Matches(UiNode element, ConditionBase condition) => condition switch
    {
        PropertyCondition propertyCondition => PropertyMatches(element, propertyCondition),
        AndCondition andCondition => andCondition.Conditions.All(child => Matches(element, child)),
        OrCondition orCondition => orCondition.Conditions.Any(child => Matches(element, child)),
        NotCondition notCondition => !Matches(element, notCondition.Condition),
        TrueCondition => true,
        FalseCondition => false,
        _ => throw new NotImplementedException($"Condition type {condition.GetType().Name} is not supported."),
    };

    private bool PropertyMatches(UiNode element, PropertyCondition propertyCondition)
    {
        var actual = GetPropertyValue(element, propertyCondition.Property);
        return EqualityComparer<object?>.Default.Equals(actual, propertyCondition.Value);
    }

    private object? GetPropertyValue(UiNode element, PropertyId propertyId)
    {
        try { return element.GetPropertyValue(propertyId); }
        catch { return null; }
    }
}