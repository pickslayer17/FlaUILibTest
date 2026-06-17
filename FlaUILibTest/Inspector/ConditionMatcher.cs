using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;

namespace FlaUILibTest.Inspector;
public static class ConditionMatcher
{
    public static bool Matches(AutomationElement element, ConditionBase condition)
    {
        return condition switch
        {
            PropertyCondition pc => MatchProperty(element, pc),
            AndCondition ac => ac.Conditions.All(c => Matches(element, c)),
            OrCondition oc => oc.Conditions.Any(c => Matches(element, c)),
            NotCondition nc => !Matches(element, nc.Condition),
            _ => false
        };
    }

    private static bool MatchProperty(AutomationElement element, PropertyCondition pc)
    {
        try
        {
            var actual = element.FrameworkAutomationElement
                .GetPropertyValue(pc.Property);

            if (actual == null && pc.Value == null) return true;
            if (actual == null || pc.Value == null) return false;

            return actual.Equals(pc.Value);
        }
        catch
        {
            return false;
        }
    }
}
