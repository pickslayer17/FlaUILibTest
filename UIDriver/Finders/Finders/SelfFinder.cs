using Interop.UIAutomationClient;

namespace UIDriver.Finders.Finders;

public sealed class SelfFinder : IFinder
{
    private readonly UIBy _elementBy;
    public UIBy BY { get => _elementBy; }

    public SelfFinder(UIBy elementBy) => _elementBy = elementBy;

    public UIAutomationElement? Find(UIAutomationElement source)
    {
        var found = source.Element.FindFirst(TreeScope.TreeScope_Descendants, _elementBy.SelfCondition!);
        return found is null ? null : new UIAutomationElement(found);
    }

    public UIAutomationElement[] FindAll(UIAutomationElement source)
    {
        var found = source.Element.FindAll(TreeScope.TreeScope_Descendants, _elementBy.SelfCondition!);
        var result = new List<UIAutomationElement>();
        for (var i = 0; i < found.Length; i++)
            result.Add(new UIAutomationElement(found.GetElement(i)));

        return result.ToArray();
    }
}
