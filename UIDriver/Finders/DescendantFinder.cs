using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;

namespace UIDriver;

public sealed class DescendantFinder : IFinder
{
    private readonly ConditionBase _element;

    public DescendantFinder(ConditionBase element) => _element = element;

    public AutomationElementObject? Find(AutomationElementObject source)
    {
        var found = source.Element.FindFirstDescendant(_element);
        return found is null ? null : new AutomationElementObject(found);
    }

    public AutomationElementObject[] FindAll(AutomationElementObject source)
        => source.Element.FindAllDescendants(_element).Select(e => new AutomationElementObject(e)).ToArray();
}
