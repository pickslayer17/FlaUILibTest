namespace UIDriver.CacheManagement;

public sealed class NodeSnapshot
{
    public required int[] RunTimeId { get; init; }
    public required int ControlType { get; init; }
    public required string? Name { get; init; }

    public required NodeChangeState ChangeState { get; init; }
    public required int? ChangedAtIteration { get; init; }

    public required IReadOnlyList<NodeSnapshot> Children { get; init; }
}
