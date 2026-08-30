using UIDriver.Constants;

namespace UIDriver.CustomModels;

public class RunTimeId
{
    public int[] Id { get; init; }
    public RunTimeIdStates State { get; init; }

    private readonly int _hashCode;

    public RunTimeId(int[] id, RunTimeIdStates state)
    {
        Id = id;
        State = state;
        _hashCode = ComputeHashCode(Id);
    }

    public override string ToString() => string.Join(",", Id);

    public string ToHexString() => Id.ToHexString();

    public override bool Equals(object? obj) => obj is RunTimeId other && Id.SequenceEqual(other.Id);

    public override int GetHashCode() => _hashCode;

    private static int ComputeHashCode(int[] id)
    {
        var hash = new HashCode();
        foreach (var part in id) hash.Add(part);
        return hash.ToHashCode();
    }
}
