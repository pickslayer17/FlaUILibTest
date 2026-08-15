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
    private IUIAutomationElement _cachedWindow;
    private readonly List<UiNode> _collectedTrees = [];

    public UICachedTreeManager(IUIAutomation automation)
    {
        _automation = automation;
    }

    public void InitCachedTree(IUIAutomationElement window)
    {
        var cacheRequest = GetCacheRequest(CachedProperties);
        _cachedWindow = window.BuildUpdatedCache(cacheRequest);

        _cachedTree = new UICachedTree(_cachedWindow);
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
            var sourceRID = source.Element.GetCachedPropertyValue((int)UiaProperty.RuntimeId) as int[];

            switch (changeType)
            {
                case StructureChangeType.StructureChangeType_ChildAdded:
                    HandleChildAdded(source);
                    break;
                case StructureChangeType.StructureChangeType_ChildRemoved:
                    break;
                case StructureChangeType.StructureChangeType_ChildrenInvalidated:
                    HandleChildrenInvalidated(source, sourceRID);
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

    private void HandleChildAdded(UIAutomationElement addedChild)
    {
        var parentElement = _automation.RawViewWalker.GetParentElement(addedChild.Element);
        var parentNode = new UiNode { Element = parentElement };

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var addedChildTree = _cachedTree.BuildUINodeTree(addedChild.Element, parentNode);
        stopwatch.Stop();
        Console.WriteLine($"ADDED: BuildUINodeTree took {stopwatch.ElapsedMilliseconds} ms");

        _collectedTrees.Add(addedChildTree);
    }

    private void HandleChildrenInvalidated(UIAutomationElement invalidatedParent, int[]? sourceRID)
    {
        if(sourceRID == null || sourceRID.Length == 0)
            return;

        var cacheRequest = GetCacheRequest(CachedProperties);
        invalidatedParent.Element.BuildUpdatedCache(cacheRequest);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var invalidatedParentTree = _cachedTree.BuildUINodeTree(invalidatedParent.Element, null);
        stopwatch.Stop();
        Console.WriteLine($"INVALIDATED: BuildUINodeTree took {stopwatch.ElapsedMilliseconds} ms");

        _collectedTrees.Add(invalidatedParentTree);
    }
}
