using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;

namespace UIDriver;

public sealed class ParentFinder : IFinder
{
    private readonly ConditionBase _parent;
    private readonly ConditionBase _element;

    public ParentFinder(ConditionBase parent, ConditionBase element)
    {
        _parent = parent;
        _element = element;
    }

    public AutomationElement? Find(AutomationElement source)
        => source.FindFirstDescendant(_parent)?.FindFirstDescendant(_element);

    public AutomationElement[] FindAll(AutomationElement source) => throw new NotImplementedException();
}
