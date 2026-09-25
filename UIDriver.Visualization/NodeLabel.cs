using UIDriver.Tree.Snapshots;

namespace UIDriver.Visualization;

internal static class NodeLabel
{
    public static string Format(NodeSnapshot node)
    {
        var label = $"[{node.ControlType}] name='{node.Name}' [{FormatRunTimeId(node)}]";
        if (node.Change.State == NodeChangeState.Original)
            return label;

        return $"{label} <{node.Change.State}@{node.Change.Version}>";
    }

    private static string FormatRunTimeId(NodeSnapshot node) => node.RunTimeId?.ToHexString() ?? "no RID";
}
