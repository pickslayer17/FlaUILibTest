namespace UIDriver.Uia;

public sealed class RunTimeId : IEquatable<RunTimeId>
{
    private readonly int[] _id;
    private readonly int _hashCode;

    private RunTimeId(int[] id)
    {
        _id = (int[])id.Clone();
        _hashCode = ComputeHashCode(_id);
    }

    public static RunTimeId? FromArray(int[]? id) => id is { Length: > 0 } ? new RunTimeId(id) : null;

    internal int[] ToArray() => (int[])_id.Clone();

    public string ToHexString() => _id.ToHexString();

    public string ToDisplayString() => _id.ToDisplayString();

    public override string ToString() => ToHexString();

    public bool Equals(RunTimeId? other) => other is not null && _id.RuntimeIdEquals(other._id);

    public override bool Equals(object? obj) => Equals(obj as RunTimeId);

    public override int GetHashCode() => _hashCode;

    private static int ComputeHashCode(int[] id)
    {
        var hash = new HashCode();
        foreach (var part in id) hash.Add(part);
        return hash.ToHashCode();
    }
}
