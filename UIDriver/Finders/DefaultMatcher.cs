using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Identifiers;

namespace UIDriver;

public sealed class DefaultMatcher : IMatcher
{
    private readonly Dictionary<PropertyId, object?> _snapshot = new();

    public bool Matches(AutomationElement element, BY condition)
    {
        if (condition.SelfCondition is null) return false;

        CaptureProperties(element);
        return Matches(condition.SelfCondition);
    }

    private void CaptureProperties(AutomationElement element)
    {
        foreach (var propertyId in UiaPropertyHelper.AllProperties)
        {
            try { _snapshot[propertyId] = element.FrameworkAutomationElement.GetPropertyValue(propertyId); }
            catch { _snapshot[propertyId] = null; }
        }
    }

    private bool Matches(ConditionBase condition) => condition switch
    {
        PropertyCondition pc => MatchProperty(pc),
        AndCondition ac => ac.Conditions.All(Matches),
        OrCondition oc => oc.Conditions.Any(Matches),
        NotCondition nc => !Matches(nc.Condition),
        TrueCondition => true,
        FalseCondition => false,
        _ => false
    };

    private bool MatchProperty(PropertyCondition pc)
    {
        var actual = _snapshot.GetValueOrDefault(pc.Property);

        if (actual is null && pc.Value is null) return true;
        if (actual is null || pc.Value is null) return false;

        return actual.Equals(pc.Value);
    }
}
