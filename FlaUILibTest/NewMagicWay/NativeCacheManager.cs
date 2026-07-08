using Interop.UIAutomationClient;

public sealed class NativeCacheManager
{
    private readonly IUIAutomation _automation;

    public NativeCacheManager(IUIAutomation automation)
    {
        _automation = automation;
    }

    public IUIAutomationElementArray Find(IUIAutomationElement root, IUIAutomationCondition condition, int[] propertyIds, IUIAutomationCondition treeFilter)
    {
        var cacheRequest = _automation.CreateCacheRequest();
        cacheRequest.TreeScope = TreeScope.TreeScope_Subtree; 
        cacheRequest.TreeFilter = treeFilter; 
        cacheRequest.AutomationElementMode = AutomationElementMode.AutomationElementMode_Full;
        foreach (var propertyId in propertyIds)
            cacheRequest.AddProperty(propertyId);
        
        var result = root.FindAllBuildCache(TreeScope.TreeScope_Subtree, condition, cacheRequest);

        return result;
    }
}
