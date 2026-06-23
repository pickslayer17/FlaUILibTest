using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;

namespace UIDriver;

// Реляционная стратегия: элемент внутри родителя — весь относительный поиск ОДНИМ проходом.
// Нет родителя → нет элемента. Не делится на несколько Watch'ей и не имеет своего отдельного таймаута.
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
