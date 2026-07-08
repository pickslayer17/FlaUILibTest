using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.UIA3.Converters;

public sealed class NativePropertyMatcher
{
    private readonly ConditionBase _condition;

    public NativePropertyMatcher(ConditionBase condition)
    {
        _condition = condition;
    }

    public bool Matches(Interop.UIAutomationClient.IUIAutomationElement element) => Matches(element, _condition);

    private bool Matches(Interop.UIAutomationClient.IUIAutomationElement element, ConditionBase condition) => condition switch
    {
        PropertyCondition propertyCondition => PropertyMatches(element, propertyCondition),
        AndCondition andCondition => andCondition.Conditions.All(child => Matches(element, child)),
        OrCondition orCondition => orCondition.Conditions.Any(child => Matches(element, child)),
        NotCondition notCondition => !Matches(element, notCondition.Condition),
        TrueCondition => true,
        FalseCondition => false,
        _ => throw new NotImplementedException($"Condition type {condition.GetType().Name} is not supported."),
    };

    private bool PropertyMatches(Interop.UIAutomationClient.IUIAutomationElement element, PropertyCondition propertyCondition)
    {
        var actual = Normalize(GetPropertyValue(element, propertyCondition.Property.Id));
        var expected = Normalize(propertyCondition.Value);
        return Equals(actual, expected);
    }

    private static object? Normalize(object? value) => value switch
    {
        ControlType controlType => ControlTypeConverter.ToControlTypeNative(controlType),
        Enum enumValue => Convert.ToInt32(enumValue),
        _ => value
    };

    private object? GetPropertyValue(Interop.UIAutomationClient.IUIAutomationElement element, int propertyId)
    {
        try { return element.GetCurrentPropertyValue(propertyId); }
        catch { return null; }
    }
}
