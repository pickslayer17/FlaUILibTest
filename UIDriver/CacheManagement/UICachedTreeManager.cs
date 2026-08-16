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
    private readonly List<Branch> _collectedBranches = [];

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

            Console.WriteLine($"iteration={_collectedBranches.Count} | source=[{ToHex(sourceRID)}] | target=[{ToHex(runtimeId)}]");
        }
    }

    private static string ToHex(int[]? runtimeId)
    {
        if (runtimeId == null)
            return "";

        return string.Join(",", runtimeId.Select(part => part.ToString("X")));
    }

    private void HandleChildAdded(UIAutomationElement addedChild)
    {
        var parentElement = _automation.RawViewWalker.GetParentElement(addedChild.Element);
        var parentNode = new UiNode { Element = parentElement };

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var addedChildTree = _cachedTree.BuildUINodeTree(addedChild.Element);
        stopwatch.Stop();
        Console.WriteLine($"ADDED: BuildUINodeTree took {stopwatch.ElapsedMilliseconds} ms");

        var branch = new HeeledBranch(parentNode, addedChildTree);
        _collectedBranches.Add(branch);
        PushBranchToVisualizer($"ADDED #{_collectedBranches.Count} [{new RunTimeId(parentElement).ToHexString()}]", branch);
    }

    private void HandleChildAddedByRuntimeId(int[] runtimeId)
    {
        var element = _cachedWindow.FindFirst(TreeScope.TreeScope_Subtree, RuntimeIdCondition(runtimeId));
        if(element == null)
        {
            Console.WriteLine($"ADDED by runtimeId [{ToHex(runtimeId)}]: not found in cachedWindow");
            return;
        }

        var parentElement = _automation.RawViewWalker.GetParentElement(element);
        var parentNode = new UiNode { Element = parentElement };

        var addedChildTree = _cachedTree.BuildUINodeTree(element);

        var branch = new HeeledBranch(parentNode, addedChildTree);
        _collectedBranches.Add(branch);
        PushBranchToVisualizer($"ADDED(byRID) #{_collectedBranches.Count} [{new RunTimeId(parentElement).ToHexString()}]", branch);
    }

    private IUIAutomationCondition RuntimeIdCondition(int[] runtimeId)
    {
        return _automation.CreatePropertyCondition((int)UiaProperty.RuntimeId, runtimeId);
    }

    private void HandleChildrenInvalidated(UIAutomationElement invalidatedParent, int[]? sourceRID)
    {
        if(sourceRID == null || sourceRID.Length == 0)
            return;

        var cacheRequest = GetCacheRequest(CachedProperties);
        invalidatedParent.Element.BuildUpdatedCache(cacheRequest);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var invalidatedParentTree = _cachedTree.BuildUINodeTree(invalidatedParent.Element);
        stopwatch.Stop();
        Console.WriteLine($"INVALIDATED: BuildUINodeTree took {stopwatch.ElapsedMilliseconds} ms");

        var branch = new Branch(invalidatedParentTree);
        _collectedBranches.Add(branch);
        PushBranchToVisualizer($"INVALIDATED #{_collectedBranches.Count} [{invalidatedParentTree.RunTimeId.ToHexString()}]", branch);
    }

    private void PushBranchToVisualizer(string title, Branch branch)
    {
        Task.Run(() => UIDriver.Visualization.TreeVisualizer.AddTree(title, branch.Tree));
    }

    public void PrintCollectedTreesParents()
    {
        for (var i = 0; i < _collectedBranches.Count; i++)
        {
            var branch = _collectedBranches[i];
            var top = branch is HeeledBranch heeled ? heeled.Heel : branch.Tree;
            var topRID = new RunTimeId(top.Element);

            var exists = _cachedTree.GetNode(n => n.RunTimeId.Equals(topRID)) != null;

            for (var j = 0; j < i && !exists; j++)
            {
                var previous = _collectedBranches[j];
                exists = ContainsRuntimeId(previous.Tree, topRID);
            }

            Console.WriteLine($"top=[{topRID.ToHexString()}] exists={exists}");
        }
    }

    private static bool ContainsRuntimeId(UiNode node, RunTimeId runtimeId)
    {
        if (node == null)
            return false;

        if (node.RunTimeId != null && node.RunTimeId.Equals(runtimeId))
            return true;

        foreach (var child in node.Children ?? [])
        {
            if (ContainsRuntimeId(child, runtimeId))
                return true;
        }

        return false;
    }
}
