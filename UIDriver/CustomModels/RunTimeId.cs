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

                throw new NotImplementedException("Unexpected single-element RuntimeId value: " + Id[0]);
            }

            return RunTimeIdStates.Valid;
        }
    }
    public int[] Id { get; init; }

    public RunTimeId(int[] id)
    {
        Id = id ?? [(int)RunTimeIdStates.Null];
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
    }

    public override string ToString() => string.Join(",", Id);

    public override bool Equals(object? obj) => obj is RunTimeId other && Id.SequenceEqual(other.Id);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var part in Id) hash.Add(part);
        return hash.ToHashCode();
    }
}