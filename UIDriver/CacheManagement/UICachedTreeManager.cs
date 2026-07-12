using Interop.UIAutomationClient;
using UIDriver;
using UIDriver.Constants;
using UIDriver.Interfaces;

public class UICachedTreeManager : IStructureChangedListener
{
    private static readonly int[] CachedProperties =
    [
        (int)UiaProperty.ControlType,
        (int)UiaProperty.Name,
        (int)UiaProperty.ClassName,
        (int)UiaProperty.AutomationId
    ];

    private readonly IUIAutomation _automation;
    private UICachedTree _cachedTree;

    public UICachedTreeManager(IUIAutomation automation)
    {
        _automation = automation;
    }

    public void InitCachedTree(IUIAutomationElement window)
    {
        var cacheRequest = GetCacheRequest(CachedProperties);
        var cachedWindow = window.BuildUpdatedCache(cacheRequest);

        _cachedTree = new UICachedTree(cachedWindow);
    }

    public UiNode Tree => _cachedTree.Tree;

    public Task<UIAutomationElement> FindFirst(UIBy by)
    {
        return Task.FromResult<UIAutomationElement>(null!);
    }

    private IUIAutomationCacheRequest GetCacheRequest(int[] propertyIds)
    {
        var cacheRequest = _automation.CreateCacheRequest();
        cacheRequest.TreeScope = TreeScope.TreeScope_Subtree;
        cacheRequest.AutomationElementMode = AutomationElementMode.AutomationElementMode_Full;
        foreach (var propertyId in propertyIds)
            cacheRequest.AddProperty(propertyId);

        return cacheRequest;
    }

    public void NotifyOnStructureChanged(UIAutomationElement source, StructureChangeType changeType, int[] runtimeId)
    {
        if (changeType == StructureChangeType.StructureChangeType_ChildrenInvalidated)
        {
        }
    }
}
