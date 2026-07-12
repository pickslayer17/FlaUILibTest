using FlaUI.UIA3.Converters;
using Interop.UIAutomationClient;
using UIDriver;
using UIDriver.Enums;
using UIDriver.Interfaces;

public class UICachedTreeManager : IStructureChangedListener
{
    private readonly IUIAutomation _automation;
    private UICachedTree _cachedTree;

    public UICachedTreeManager(IUIAutomation automation)
    {
        _automation = automation;
    }

    public void InitCachedTree(IUIAutomationElement desktop, IUIAutomationCondition windowCondition, int[] propertyIds)
    {
        var cachedRequest = GetCacheRequest(TreeScope.TreeScope_Subtree, null, propertyIds);
        var found = desktop.FindFirstBuildCache(TreeScope.TreeScope_Descendants, windowCondition, cachedRequest);
        if (found != null)
        {
            _cachedTree = new UICachedTree(found);
            return;
        }

        throw new Exception("Failed to initialize cached tree. No matching element found.");
    }

    public IUIAutomationCacheRequest GetCacheRequest(
        TreeScope cacheTreeScope,
        IUIAutomationCondition TreeFilter,
        int[] propertyIds)
    {
        var cacheRequest = _automation.CreateCacheRequest();
        cacheRequest.TreeScope = TreeScope.TreeScope_Subtree;
        cacheRequest.TreeFilter = null;
        cacheRequest.AutomationElementMode = AutomationElementMode.AutomationElementMode_Full;
        foreach (var propertyId in propertyIds)
            cacheRequest.AddProperty(propertyId);

        return cacheRequest;
    }

    public IUIAutomationElement FindFirst(TreeScope treeScope, IUIAutomationCondition condition, IUIAutomationCacheRequest cacheRequest)
    {
        var result = _cachedTree.CachedWindow.FindFirstBuildCache(treeScope, condition, cacheRequest);

        return result;
    }

    public IUIAutomationElementArray FindAll(TreeScope treeScope, IUIAutomationCondition condition, IUIAutomationCacheRequest cacheRequest)
    {
        var result = _cachedTree.CachedWindow.FindAllBuildCache(treeScope, condition, cacheRequest);

        return result;
    }

    public void NotifyOnStructureChanged(UIAutomationElement source, FlaUI.Core.Definitions.StructureChangeType changeType, int[] runtimeId)
    {
        if (changeType == FlaUI.Core.Definitions.StructureChangeType.ChildrenInvalidated)
        {
            var cacheRequest = GetCacheRequest(TreeScope.TreeScope_Subtree, null, [ (int)NativePropertyIds.UIA_NamePropertyId ]);
        }


        source.Element.ToNative();

    }
}