using System.CodeDom;

namespace UIDriver.Finders.Finders;

public class AncestorFinder : FinderDecoratorBase
{
    public AncestorFinder(IFinder inner) : base(inner)
    {
    }

    public override UIAutomationElement? Find(UIAutomationElement source)
    {
        throw new NotImplementedException();
    }

    public override UIAutomationElement[] FindAll(UIAutomationElement source)
    {
        throw new NotImplementedException();
    }
}
