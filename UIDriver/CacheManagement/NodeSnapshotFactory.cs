namespace UIDriver.CacheManagement;

public static class NodeSnapshotFactory
{
    public static TreeSnapshot ToTreeSnapshot(UiNode root, int iteration) => new()
    {
        Iteration = iteration,
        TakenAt = DateTime.Now,
        Root = ToNodeSnapshot(root)
    };

    public static NodeSnapshot ToNodeSnapshot(UiNode node)
    {
        var children = new List<NodeSnapshot>();
        foreach (var child in node.Children ?? [])
            children.Add(ToNodeSnapshot(child));

        return new NodeSnapshot
        {
            RunTimeId = node.RunTimeId?.Id ?? [],
            ControlType = node.ControlType,
            Name = node.Name,
            ChangeState = node.ChangeState,
            ChangedAtIteration = node.ChangedAtIteration,
            Children = children
        };
    }
}
