namespace UIDriver.Tree.Snapshots;

public sealed class NodeChangeHistory
{
    private readonly Dictionary<UiNode, NodeChange> _changes = new();

    public void Record(UiNode branchRoot, NodeChangeState state, int version)
    {
        var change = new NodeChange(state, version);
        foreach (var node in branchRoot.Traverse())
            _changes[node] = change;
    }

    public NodeChange Get(UiNode node) => _changes.GetValueOrDefault(node, NodeChange.Original);
}
