namespace UIDriver.CacheManagement;

public sealed class TreeSnapshot
{
    public required int Iteration { get; init; }
    public required DateTime TakenAt { get; init; }
    public required NodeSnapshot Root { get; init; }
}
