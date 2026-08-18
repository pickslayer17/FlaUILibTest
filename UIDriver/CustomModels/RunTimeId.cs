using Interop.UIAutomationClient;
using UIDriver.Constants;

namespace UIDriver.CustomModels;

public class RunTimeId
{
    public RunTimeIdStates State
    {
        get
        {
            if (Id.Length == 0)
            {
                return RunTimeIdStates.Empty;
            }

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
        Id = GetRunTimeId(element);
        _hashCode = ComputeHashCode(Id);
    }

    protected virtual int[] GetRunTimeId(IUIAutomationElement element)
    {
        try
        {
            if (element.GetCachedPropertyValue((int)UiaProperty.RuntimeId) is int[] cached)
                return cached;
        }
        catch
        {
        }

        try
        {
            return (int[]?)element.GetRuntimeId() ?? [(int)RunTimeIdStates.Null];
        }
        catch
        {
            return [(int)RunTimeIdStates.ErrorTryingGet];
        }
    }

    public override string ToString() => string.Join(",", Id);

    public string ToHexString() => string.Join(",", Id.Select(part => part.ToString("X")));

    public override bool Equals(object? obj) => obj is RunTimeId other && Id.SequenceEqual(other.Id);

    public override int GetHashCode() => _hashCode;

    private static int ComputeHashCode(int[] id)
    {
        var hash = new HashCode();
        foreach (var part in id) hash.Add(part);
        return hash.ToHashCode();
    }
}