namespace UIDriver.Tree.Snapshots;

public static class NodeSnapshotFactory
{
    public static NodeSnapshot Create(UiNode node, NodeChangeHistory changeHistory) => new(
        node.RunTimeId,
        node.ControlType,
        node.Name,
        node.IsDirty,
        changeHistory.Get(node),
        node.Children.Select(child => Create(child, changeHistory)).ToArray());
}
