namespace UIDriver.Finders.Finders;

public sealed class SelfFinder : IFinder
{
    private readonly UIBy _elementBy;
    public UIBy BY { get => _elementBy; }

    public SelfFinder(UIBy elementBy) => _elementBy = elementBy;

    public UIAutomationElement? Find(UIAutomationElement source)
    {
        //Driver._automation. TreeWalkerFactory.GetControlViewWalker().Get
        // We will use TreeWalker for all searcg needs, check selfcondition with Mather.


        var found = source.Element.FindFirstDescendant(_elementBy.SelfCondition!);
        return found is null ? null : new UIAutomationElement(found);
    }

    public UIAutomationElement[] FindAll(UIAutomationElement source)
        => source.Element.FindAllDescendants(_elementBy.SelfCondition!).Select(e => new UIAutomationElement(e)).ToArray();
}
