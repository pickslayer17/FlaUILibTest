namespace UIDriver;

public abstract class FinderDecorator : IFinder
{
    protected readonly IFinder Inner;

    protected FinderDecorator(IFinder inner) => Inner = inner;

    public abstract AutomationElementObject? Find(AutomationElementObject source);
    public abstract AutomationElementObject[] FindAll(AutomationElementObject source);
}
