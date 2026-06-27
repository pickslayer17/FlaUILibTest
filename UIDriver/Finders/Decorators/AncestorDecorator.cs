namespace UIDriver.Finders.Decorators;

public class AncestorDecorator : FinderDecoratorBase
{
    public AncestorDecorator(IFinder inner) : base(inner)
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
