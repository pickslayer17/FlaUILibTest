namespace UIDriver.Finders;

public abstract class FinderDecoratorBase : IFinder
{
    protected readonly IFinder Inner;
    protected readonly BY innerBy;

    protected FinderDecoratorBase(IFinder inner)
    {
        Inner = inner;
    }

    public abstract AutomationElementObject? Find(AutomationElementObject source);
    public abstract AutomationElementObject[] FindAll(AutomationElementObject source);
    public abstract bool Matches(AutomationElementObject source);
}
