using UIDriver.Constants;

namespace UIDriver.CustomModels;

public sealed class CachedRunTimeId : RunTimeId
{
    public CachedRunTimeId(int[] id, RunTimeIdStates state) : base(id, state)
    {
    }
}
