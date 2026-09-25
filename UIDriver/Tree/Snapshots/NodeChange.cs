namespace UIDriver.Tree.Snapshots;

public sealed record NodeChange(NodeChangeState State, int Version)
{
    public static NodeChange Original { get; } = new(NodeChangeState.Original, 0);
}
