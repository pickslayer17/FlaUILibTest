using FlaUI.Core.AutomationElements;

namespace UIDriver;

public sealed class DescendantFinder : IFinder
{
    private readonly BY _elementBy;

    public DescendantFinder(BY elementBy) => _elementBy = elementBy;

    public AutomationElementObject? Find(AutomationElementObject source)
    {
        var found = source.Element.FindFirstDescendant(_elementBy.SelfCondition!);
        return found is null ? null : new AutomationElementObject(found);
    }

    public AutomationElementObject[] FindAll(AutomationElementObject source)
        => source.Element.FindAllDescendants(_elementBy.SelfCondition!).Select(e => new AutomationElementObject(e)).ToArray();

    public bool Matches(AutomationElementObject source)
        => new DefaultMatcher().Matches(source.Element, _elementBy);
}
