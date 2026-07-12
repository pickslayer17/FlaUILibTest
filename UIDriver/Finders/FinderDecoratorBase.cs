namespace UIDriver.Finders;

public abstract class FinderDecoratorBase : IFinder
{
    protected readonly IFinder Inner;

    protected FinderDecoratorBase(IFinder inner)
    {
        Inner = inner;
    }

    public abstract UIAutomationElement? Find(UIAutomationElement source);
    public abstract UIAutomationElement[] FindAll(UIAutomationElement source);
}
