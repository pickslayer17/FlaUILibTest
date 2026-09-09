using Interop.UIAutomationClient;

namespace UIDriver.NewCacheManagement;

public static class CacheRequestFactory
{
    public static int[] CachedProperties =
    [
        (int)UiaProperty.RuntimeId,
        (int)UiaProperty.ControlType,
        (int)UiaProperty.Name
    ];

    public static TreeScope TreeScope = TreeScope.TreeScope_Subtree;
    public static AutomationElementMode AutomationElementMode = AutomationElementMode.AutomationElementMode_Full;

    public static IUIAutomationCacheRequest BuildCacheRequest(IUIAutomation automation)
    {
        var cacheRequest = automation.CreateCacheRequest();
        cacheRequest.TreeScope = TreeScope;
        cacheRequest.AutomationElementMode = AutomationElementMode;
        foreach (var propertyId in CachedProperties)
            cacheRequest.AddProperty(propertyId);

        return cacheRequest;
    }
}
