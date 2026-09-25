using Interop.UIAutomationClient;

namespace CacheManagement.Navigation;

public sealed class UiNodePropMatcher
{
    private readonly IUIAutomationCondition _condition;

    public UiNodePropMatcher(IUIAutomationCondition condition)
    {
        _condition = condition;
    }

    public bool Matches(UiNode element)
    {
        return false; // to do: native condition matching
    }
}
