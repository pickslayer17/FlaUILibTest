using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;

namespace UIDriver;

// Базовая стратегия: элемент-потомок source по условию.
public sealed class DescendantFinder : IFinder
{
    private readonly ConditionBase _element;

    public DescendantFinder(ConditionBase element) => _element = element;

    public AutomationElement? Find(AutomationElement source) => source.FindFirstDescendant(_element);

    public AutomationElement[] FindAll(AutomationElement source) => source.FindAllDescendants(_element);
}
