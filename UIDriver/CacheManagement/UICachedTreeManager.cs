using Interop.UIAutomationClient;
using UIDriver;
using UIDriver.CacheManagement;
using UIDriver.CustomModels;
using UIDriver.Interfaces;
using UIDriver.Visualization;

public class UICachedTreeManager : IStructureChangedListener, IPropertyChangedListener
{
    private static readonly int[] CachedProperties =
    [
        (int)UiaProperty.RuntimeId,
        (int)UiaProperty.ControlType,
        (int)UiaProperty.Name
    ];

    private readonly IUIAutomation _automation;
    private readonly ContainerId _containerId;
    private readonly ITreeSnapshotSink _snapshotSink;
    private UICachedTree _cachedTree;
    private IUIAutomationElement _cachedWindow;
    private readonly List<Branch> _collectedBranches = [];
    private int _iteration;

    public UICachedTreeManager(IUIAutomation automation, ContainerId containerId, ITreeSnapshotSink snapshotSink)
    {
        _automation = automation;
        _containerId = containerId;
        _snapshotSink = snapshotSink;
    }

    public void InitCachedTree(IUIAutomationElement window)
    {
        var cacheRequest = GetCacheRequest(CachedProperties);
        _cachedWindow = window.BuildUpdatedCache(cacheRequest);

        _cachedTree = new UICachedTree(_cachedWindow);
    }

    public UiNode Tree => _cachedTree.Tree;

    public void PublishInitialSnapshot(string title)
    {
        var snapshot = _cachedTree.Commit(++_iteration);
        Task.Run(() => _snapshotSink.OnSnapshot(_containerId, title, snapshot));
    }

    public Task<IUIAutomationElement> FindFirst(UIBy by)
    {
        return Task.FromResult<IUIAutomationElement>(null!);
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
    private DateTime _previousTimestamp;
    public void NotifyOnStructureChanged(IUIAutomationElement source, StructureChangeType changeType, int[] runtimeId)
    {
        lock (notifyLock)
        {
            var sourceRID = source.GetCachedPropertyValue((int)UiaProperty.RuntimeId) as int[];
            var controlType = source.GetCachedPropertyValue((int)UiaProperty.ControlType) as int?;
            var name = source.GetCachedPropertyValue((int)UiaProperty.Name) as string;

            switch (changeType)
            {
                case StructureChangeType.StructureChangeType_ChildAdded:
                    HandleChildAdded(source);
                    break;
                case StructureChangeType.StructureChangeType_ChildRemoved:
                    Console.WriteLine("REMOVED");
                    break;
                case StructureChangeType.StructureChangeType_ChildrenInvalidated:
                    HandleChildrenInvalidated(source, sourceRID);
                    break;
                case StructureChangeType.StructureChangeType_ChildrenReordered:
                    Console.WriteLine("REORDERED");
                    break;
                case StructureChangeType.StructureChangeType_ChildrenBulkRemoved:
                    Console.WriteLine("BULK_REMOVED");
                    break;
                case StructureChangeType.StructureChangeType_ChildrenBulkAdded:
                    Console.WriteLine("BULK_ADDED");
                    break;
                default:
                    throw new NotImplementedException();
            }

            var now = DateTime.Now;
            var delta = _previousTimestamp == default ? TimeSpan.Zero : now - _previousTimestamp;
            _previousTimestamp = now;

            Console.WriteLine($"[{now:HH:mm:ss.fff}] (+{delta.TotalMilliseconds:F0} ms) iteration={_collectedBranches.Count} | source=[{ToHex(sourceRID)}] | target=[{ToHex(runtimeId)}] | controlType=[{ControlType(controlType)}] | name=[{name}]");
        }
    }

    public void NotifyOnPropertyChanged(IUIAutomationElement source, int propertyId, object newValue)
    {
        lock (notifyLock)
        {
            var runtimeId = source.CachedRuntimeId();
            //Console.WriteLine($"PROPERTY CHANGED | source=[{runtimeId.ToHexString()}] | property={PropertyName(propertyId)} | newValue={newValue}");
        }
    }

    private string ControlType(int? controlTypeId)
    {
        if (controlTypeId == null)
            return "";

        return Enum.IsDefined(typeof(UiaControlType), controlTypeId)
            ? ((UiaControlType)controlTypeId).ToString()
            : controlTypeId.ToString();
    }

    private string PropertyName(int propertyId)
    {
        return Enum.IsDefined(typeof(UiaProperty), propertyId)
            ? ((UiaProperty)propertyId).ToString()
            : propertyId.ToString();
    }

    private static string ToHex(int[]? runtimeId)
    {
        if (runtimeId == null)
            return "";

        return string.Join(",", runtimeId.Select(part => part.ToString("X")));
    }

    private void HandleChildAdded(IUIAutomationElement addedChild)
    {
        var parentElement = _automation.RawViewWalker.GetParentElement(addedChild);
        var parentNode = new UiNode { Element = parentElement };

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var addedChildTree = _cachedTree.BuildUINodeTree(addedChild);
        stopwatch.Stop();
        Console.WriteLine($"ADDED: BuildUINodeTree took {stopwatch.ElapsedMilliseconds} ms");

        var branch = new HeeledBranch(parentNode, addedChildTree);
        _collectedBranches.Add(branch);
        PublishSnapshot($"ADDED #{_collectedBranches.Count} [{parentElement.LiveRuntimeId().ToHexString()}]", branch);
    }

    private void HandleChildrenInvalidated(IUIAutomationElement invalidatedParent, int[]? sourceRID)
    {
        if(sourceRID == null || sourceRID.Length == 0)
            return;

        var cacheRequest = GetCacheRequest(CachedProperties);
        invalidatedParent.BuildUpdatedCache(cacheRequest);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var invalidatedParentTree = _cachedTree.BuildUINodeTree(invalidatedParent);
        stopwatch.Stop();
        Console.WriteLine($"INVALIDATED: BuildUINodeTree took {stopwatch.ElapsedMilliseconds} ms");

        var branch = new Branch(invalidatedParentTree);
        _collectedBranches.Add(branch);
        PublishSnapshot($"INVALIDATED #{_collectedBranches.Count} [{invalidatedParentTree.RunTimeId.ToHexString()}]", branch);
    }

    private void PublishSnapshot(string title, Branch branch)
    {
        var snapshot = NodeSnapshotFactory.ToTreeSnapshot(branch.Tree, ++_iteration);
        Task.Run(() => _snapshotSink.OnSnapshot(_containerId, title, snapshot));
    }

    public void PrintCollectedTreesParents()
    {
        for (var i = 0; i < _collectedBranches.Count; i++)
        {
            var branch = _collectedBranches[i];
            var top = branch is HeeledBranch heeled ? heeled.Heel : branch.Tree;
            var topRID = branch is HeeledBranch ? top.Element.LiveRuntimeId() : top.Element.CachedRuntimeId();

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
