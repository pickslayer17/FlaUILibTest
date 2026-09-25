using UIDriver.Tree.Snapshots;

namespace UIDriver.Visualization;

internal static class TreeNodeBuilder
{
    public static TreeNode Build(NodeSnapshot node) => Build(node, highlightedVersion: null);

    public static TreeNode Build(NodeSnapshot node, int? highlightedVersion)
    {
        var treeNode = new TreeNode(NodeLabel.Format(node))
        {
            ForeColor = NodeChangeColors.ForeColorOf(node.Change.State),
            BackColor = IsHighlighted(node, highlightedVersion) ? NodeChangeColors.Highlight : Color.Empty
        };

        foreach (var child in node.Children)
            treeNode.Nodes.Add(Build(child, highlightedVersion));

        return treeNode;
    }

    private static bool IsHighlighted(NodeSnapshot node, int? highlightedVersion)
        => node.Change.State != NodeChangeState.Original && node.Change.Version == highlightedVersion;
}
