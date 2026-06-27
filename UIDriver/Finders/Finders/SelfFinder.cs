namespace UIDriver.Finders.Finders;

public sealed class SelfFinder : IFinder
{
    private readonly BY _elementBy;
    public BY BY { get => _elementBy; }

    public SelfFinder(BY elementBy) => _elementBy = elementBy;

    public AutomationElementObject? Find(AutomationElementObject source)
    {
        //Driver._automation. TreeWalkerFactory.GetControlViewWalker().Get
        // We will use TreeWalker for all searcg needs, check selfcondition with Mather.


        var found = source.Element.FindFirstDescendant(_elementBy.SelfCondition!);
        return found is null ? null : new AutomationElementObject(found);
    }

    public AutomationElementObject[] FindAll(AutomationElementObject source)
        => source.Element.FindAllDescendants(_elementBy.SelfCondition!).Select(e => new AutomationElementObject(e)).ToArray();
}
