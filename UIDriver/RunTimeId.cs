using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;
using UIDriver.Constants;

namespace UIDriver;

public sealed class RunTimeId
{
    public RunTimeIdStates State
    {
        get
        {
            if (Id.Length == 1)
            {
                if (Id[0] == RunTimeIdStates.ErrorTryingGet.ToInt()) return RunTimeIdStates.ErrorTryingGet;
                if (Id[0] == RunTimeIdStates.Null.ToInt()) return RunTimeIdStates.Null;

                throw new NotImplementedException("Unexpected single-element RuntimeId value: " + Id[0]);
            }

            return RunTimeIdStates.Valid;
        }
    }
    private int[] Id { get; init; }

    public RunTimeId(int[] id)
    {
        Id = id ?? [RunTimeIdStates.Null.ToInt()];
    }

    public RunTimeId(AutomationElement element)
    {
        int[] runtimeId = [RunTimeIdStates.ErrorTryingGet.ToInt()];
        try
        {
            runtimeId = element.Properties.RuntimeId.ValueOrDefault ?? [RunTimeIdStates.Null.ToInt()];
        }
        catch (Exception ex)
        {
            LogEventFactory.RaiseText($"Failed to get RuntimeId for element: {ex.Message}");
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