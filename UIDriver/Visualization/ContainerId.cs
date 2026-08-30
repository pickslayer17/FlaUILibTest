namespace UIDriver.Visualization;

public sealed class ContainerId
{
    private static int _next;

    public int Value { get; }

    public ContainerId() => Value = System.Threading.Interlocked.Increment(ref _next);

    public override string ToString() => Value.ToString();
}
