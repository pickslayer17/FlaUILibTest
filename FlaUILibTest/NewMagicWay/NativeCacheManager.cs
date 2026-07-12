using Interop.UIAutomationClient;

public sealed class NativeCacheManager
{
    private readonly IUIAutomation _automation;

    public NativeCacheManager(IUIAutomation automation)
    {
        _automation = automation;
    }

    public IUIAutomationElement FindFirstBuildCache(IUIAutomationElement root, IUIAutomationCondition condition, int[] propertyIds)
    {
        var cacheRequest = _automation.CreateCacheRequest();
        cacheRequest.TreeScope = TreeScope.TreeScope_Subtree; 
        cacheRequest.AutomationElementMode = AutomationElementMode.AutomationElementMode_Full;
        foreach (var propertyId in propertyIds)
            cacheRequest.AddProperty(propertyId);
        
        var result = root.FindFirstBuildCache(TreeScope.TreeScope_Subtree, condition, cacheRequest);

        return result;
    }

    public IUIAutomationElementArray FindAllBuildCache(IUIAutomationElement root, IUIAutomationCondition condition, int[] propertyIds)
    {
        var cacheRequest = _automation.CreateCacheRequest();
        cacheRequest.TreeScope = TreeScope.TreeScope_Subtree; 
        cacheRequest.AutomationElementMode = AutomationElementMode.AutomationElementMode_Full;
        foreach (var propertyId in propertyIds)
            cacheRequest.AddProperty(propertyId);
        
        var result = root.FindAllBuildCache(TreeScope.TreeScope_Subtree, condition, cacheRequest);

        return result;
    }
}
