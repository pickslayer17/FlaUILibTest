using CacheManagement;
using Interop.UIAutomationClient;

namespace UIDriver.NewCacheManagement;

public class BranchFactory
{
    public static Branch BuildBranch(IUIAutomationElement element)
    {
        var nodeTree = BuildUINodeTreeCore(element, null);
        var branch = new Branch(nodeTree);

        return branch;
    }

    public static HeeledBranch BuildHeeledBranch(IUIAutomationElement element, IUIAutomationElement liveParent)
    {
        var parentNode = NodeFactory.NewNode(liveParent, liveRunTimeId: true);
        var nodeTree = BuildUINodeTreeCore(element, null);
        var heeledBranch = new HeeledBranch(nodeTree, parentNode);

        return heeledBranch;
    }

    public static UiNode BuildUINodeTree(IUIAutomationElement element)
    {
        return BuildUINodeTreeCore(element, null);
    }

    private static UiNode BuildUINodeTreeCore(IUIAutomationElement element, UiNode parent)
    {
        var node = NodeFactory.NewNodeWithParent(element, parent);
        var children = new List<UiNode>();
        var cachedChildren = element.GetCachedChildren();
        var childCount = cachedChildren?.Length ?? 0;

        for (var i = 0; i < childCount && cachedChildren != null; i++)
        {
            var childElement = cachedChildren.GetElement(i);
            children.Add(BuildUINodeTreeCore(childElement, node));
        }

        node.Children = children.ToArray();
        return node;
    }
}
