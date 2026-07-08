using FlaUI.UIA3;
using Interop.UIAutomationClient;

public sealed class NativeCacheManager
{
    private readonly IUIAutomation _automation;

    public NativeCacheManager(UIA3Automation automation)
    {
        _automation = automation.NativeAutomation;
    }

    public IUIAutomationElementArray Find(IUIAutomationElement root, IUIAutomationCondition condition, int[] propertyIds)
    {
        var cacheRequest = _automation.CreateCacheRequest();
        cacheRequest.TreeScope = TreeScope.TreeScope_Subtree;
        cacheRequest.AutomationElementMode = AutomationElementMode.AutomationElementMode_Full;
        foreach (var propertyId in propertyIds)
            cacheRequest.AddProperty(propertyId);

        return root.FindAllBuildCache(TreeScope.TreeScope_Subtree, condition, cacheRequest);
    }
}
