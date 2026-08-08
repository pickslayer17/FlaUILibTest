using Interop.UIAutomationClient;
using UIDriver;
using UIDriver.CustomModels;
using UIDriver.Interfaces;

public class UICachedTreeManager : IStructureChangedListener
{
    private static readonly int[] CachedProperties =
    [
        (int)UiaProperty.RuntimeId,
        (int)UiaProperty.ControlType,
        (int)UiaProperty.Name
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
    public Lock notifyLock = new Lock();
    public void NotifyOnStructureChanged(UIAutomationElement source, StructureChangeType changeType, int[] runtimeId)
    {
        lock (notifyLock)
        {
            var targetRID = runtimeId;
            var sourceRID = source.Element.GetRuntimeId();
            var sourceValid = sourceRID != null && sourceRID.Length > 0;
            var targetValid = targetRID != null && targetRID.Length > 0;
            bool anyValid = sourceValid || targetValid;
            bool? sourceAndTargetAreEquals = false;

            switch (changeType)
            {
                case StructureChangeType.StructureChangeType_ChildAdded:
                    if (anyValid)
                    {
                        if (sourceValid)
                        {
                            if (targetValid)
                            {

                                if (sourceRID.SequenceEqual(targetRID))
                                {
                                    sourceAndTargetAreEquals = true;
                                }
                                else
                                {
                                    sourceAndTargetAreEquals= false;
                                }
                            }
                            else
                            {
                            }
                        }
                    }

                    Console.WriteLine( $"{sourceValid}, {targetValid}, {sourceAndTargetAreEquals}");
                    break;
                case StructureChangeType.StructureChangeType_ChildRemoved:
                    break;
                case StructureChangeType.StructureChangeType_ChildrenInvalidated:
                    break;
                case StructureChangeType.StructureChangeType_ChildrenReordered:
                    break;
                case StructureChangeType.StructureChangeType_ChildrenBulkRemoved:
                    break;
                case StructureChangeType.StructureChangeType_ChildrenBulkAdded:
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
