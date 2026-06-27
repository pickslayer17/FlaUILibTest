namespace UIDriver.Finders.Finders;

public class AncestorFinder : FinderDecoratorBase
{
    public AncestorFinder(IFinder inner) : base(inner)
    {
    }

    public override AutomationElementObject? Find(AutomationElementObject source)
    {
        throw new NotImplementedException();
    }

    public override AutomationElementObject[] FindAll(AutomationElementObject source)
    {
        throw new NotImplementedException();
    }

    public override bool Matches(AutomationElementObject source)
    {
        throw new NotImplementedException();
    }
}
