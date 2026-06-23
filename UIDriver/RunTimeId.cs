namespace UIDriver;

public sealed class RunTimeId
{
    private readonly int[] _id;

    public RunTimeId(int[] id) => _id = id;

    public static RunTimeId FromString(string value) => new(value.Split(',').Select(int.Parse).ToArray());

    public override string ToString() => string.Join(",", _id);

    public override bool Equals(object? obj) => obj is RunTimeId other && _id.SequenceEqual(other._id);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var part in _id) hash.Add(part);
        return hash.ToHashCode();
    }
}
