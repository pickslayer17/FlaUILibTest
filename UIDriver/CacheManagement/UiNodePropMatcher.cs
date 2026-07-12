using Interop.UIAutomationClient;

namespace UIDriver.CacheManagement;

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
