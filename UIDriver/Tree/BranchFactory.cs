using UIDriver.Uia;

namespace UIDriver.Tree;

public static class BranchFactory
{
    public static Branch BuildBranch(UiaElement element)
    {
        var nodeTree = BuildUINodeTree(element);
        var branch = new Branch(nodeTree);

        return branch;
    }

    public static HeeledBranch BuildHeeledBranch(UiaElement element, UiaElement liveParent)
    {
        var heel = NodeFactory.NewHeelFromLive(liveParent);
        var nodeTree = BuildUINodeTree(element);
        var heeledBranch = new HeeledBranch(nodeTree, heel);

        return heeledBranch;
    }

    public static UiNode BuildUINodeTree(UiaElement element)
    {
        return BuildUINodeTreeCore(element, null);
    }

    private static UiNode BuildUINodeTreeCore(UiaElement element, UiNode? parent)
    {
        var node = NodeFactory.NewNodeFromCache(element);
        node.Parent = parent;
        node.Children = element.Cached.Children
            .Select(child => BuildUINodeTreeCore(child, node))
            .ToArray();

        return node;
    }
}
