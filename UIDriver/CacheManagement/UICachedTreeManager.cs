using Interop.UIAutomationClient;
using UIDriver;
using UIDriver.Constants;
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
            var a = source.Element.CurrentName;
            var b = source.Element.CurrentControlType;
            var c = source.Element.CurrentClassName;
            var d = new RunTimeId(source.Element.GetRuntimeId());
            Console.WriteLine("-------------------");
            Console.WriteLine("Name "+ a);
            Console.WriteLine("Control Type " + b);
            Console.WriteLine("ClassName " + c);
            Console.WriteLine("source RTid state " + d.State);
            Console.WriteLine("source RTid " + d);
            Console.WriteLine("cached source RTid "+ source.RunTimeId);
            Console.WriteLine("ct"+changeType.ToString());
            Console.WriteLine("add RTid"+new RunTimeId(runtimeId));
            Console.WriteLine("-------------------");
            return;
            if (changeType == StructureChangeType.StructureChangeType_ChildrenInvalidated)
            {
                return;
            }
            if (changeType == StructureChangeType.StructureChangeType_ChildAdded)
            {
                var node = _cachedTree.NodesByRunTimeId[source.RunTimeId];
                var cacheRequest = GetCacheRequest(CachedProperties);
                var updatedCachedElement = node.Element.BuildUpdatedCache(cacheRequest);

                _cachedTree.UpdateNode(node, updatedCachedElement);
            }
            if (changeType == StructureChangeType.StructureChangeType_ChildRemoved)
            {
                var node = _cachedTree.NodesByRunTimeId[source.RunTimeId];
                var cacheRequest = GetCacheRequest(CachedProperties);
                var updatedCachedElement = node.Element.BuildUpdatedCache(cacheRequest);

                _cachedTree.UpdateNode(node, updatedCachedElement);
            }
        }
    }
}
