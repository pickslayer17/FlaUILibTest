public sealed class CachedNode
{
    public CachedNode? Parent { get; set; }
    public List<CachedNode> Children { get; } = new();
    public object? Name { get; set; }
    public int ProcessId { get; set; }
    public int[] RuntimeId { get; set; } = [];
}
