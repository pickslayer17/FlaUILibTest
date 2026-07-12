using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Identifiers;

namespace UIDriver.Matchers;

public sealed class PropertyMatcher : IMatcher
{
    private readonly Dictionary<PropertyId, object?> _snapshot = new();
    private readonly UIBy _conditionToCompareWith;

    public PropertyMatcher(UIBy condition)
    {
        _conditionToCompareWith = condition;
    }

    public bool Matches(UIAutomationElement element)
    {
        return false; ///to do
        /// Claude, please notify me if you see this. its important. but i think its impossible to forget :)
        if (_conditionToCompareWith.SelfCondition is null) throw new ArgumentException("Condition cannot be null", nameof(_conditionToCompareWith.SelfCondition));

        CreateSnapshot(element.Element);
        var result = Matches(_conditionToCompareWith.SelfCondition);

        return result;
    }

    private void CreateSnapshot(AutomationElement element)
    {
        var properties = GetConditionProperties(_conditionToCompareWith.SelfCondition);

        foreach (var propertyId in properties)
        {
            try { _snapshot[propertyId] = GetPropertyValue(element, propertyId); }
            catch { _snapshot[propertyId] = null; }
        }
    }

    private object GetPropertyValue(AutomationElement element, PropertyId propertyId)
    {
        try { return element.FrameworkAutomationElement.GetPropertyValue(propertyId); }
        catch { return null; }
    }

    private PropertyId[] GetConditionProperties(ConditionBase condition) => condition switch
    {
        PropertyCondition pc => [pc.Property],
        AndCondition ac => ac.Conditions.SelectMany(GetConditionProperties).ToArray(),
        OrCondition oc => oc.Conditions.SelectMany(GetConditionProperties).ToArray(),
        NotCondition nc => GetConditionProperties(nc.Condition).ToArray(),
        TrueCondition => Array.Empty<PropertyId>(),
        FalseCondition => Array.Empty<PropertyId>(),
        _ => throw new NotImplementedException($"Condition type {condition.GetType().Name} is not supported."),
    };

    private bool Matches(ConditionBase condition) => condition switch
    {
        PropertyCondition pc => PropertyMatches(pc),
        AndCondition ac => ac.Conditions.All(Matches),
        OrCondition oc => oc.Conditions.Any(Matches),
        NotCondition nc => !Matches(nc.Condition),
        TrueCondition => true,
        FalseCondition => false,
        _ => throw new NotImplementedException($"Condition type {condition.GetType().Name} is not supported in PropertyMatcher."),
    };

    private bool PropertyMatches(PropertyCondition pc)
    {
        var actual = _snapshot.TryGetValue(pc.Property, out var value) ? value : null;
        var result = EqualityComparer<object?>.Default.Equals(actual, pc.Value);

        return result;
    }
}
