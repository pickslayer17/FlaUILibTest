using Interop.UIAutomationClient;
using UIDriver.Constants;

namespace UIDriver.CustomModels;

public sealed class RunTimeId
{
    public RunTimeIdStates State
    {
        get
        {
            if (Id.Length == 1)
            {
                if (Id[0] == (int)RunTimeIdStates.ErrorTryingGet) return RunTimeIdStates.ErrorTryingGet;
                if (Id[0] == (int)RunTimeIdStates.Null) return RunTimeIdStates.Null;
            }

            return RunTimeIdStates.Valid;
        }
    }
    public int[] Id { get; init; }

    private readonly int _hashCode;

    public RunTimeId(int[] id)
    {
        Id = id ?? [(int)RunTimeIdStates.Null];
        _hashCode = ComputeHashCode(Id);
    }

    public RunTimeId(IUIAutomationElement element)
    {
        int[] runtimeId = [(int)RunTimeIdStates.ErrorTryingGet];
        try
        {
            runtimeId = (int[]?)element.GetRuntimeId() ?? [(int)RunTimeIdStates.Null];
        }
        catch
        {
        }
        Id = runtimeId;
        _hashCode = ComputeHashCode(Id);
    }

    public override string ToString() => string.Join(",", Id);

    public override bool Equals(object? obj) => obj is RunTimeId other && Id.SequenceEqual(other.Id);

    public override int GetHashCode() => _hashCode;

    private static int ComputeHashCode(int[] id)
    {
        var hash = new HashCode();
        foreach (var part in id) hash.Add(part);
        return hash.ToHashCode();
    }
}