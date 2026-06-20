using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;

namespace FlaUILibTest.Inspector;

public class SearchManager
{
    private readonly Lock _searchLock = new();

    public AutomationElement FindFirst(AutomationElement root, ConditionBase condition)
    {
        lock (_searchLock)
        {
            return root.FindFirstDescendant(condition);
        }
    }
}
