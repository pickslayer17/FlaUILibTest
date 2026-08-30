using Interop.UIAutomationClient;
using UIDriver.Constants;

namespace UIDriver.CustomModels;

public static class AutomationElementRunTimeIdExtensions
{
    public static CachedRunTimeId CachedRuntimeId(this IUIAutomationElement element)
    {
        int[] id;
        try
        {
            id = element.GetCachedPropertyValue((int)UiaProperty.RuntimeId) as int[] ?? [];
        }
        catch
        {
            return new CachedRunTimeId([], RunTimeIdStates.ErrorTryingGet);
        }

        return new CachedRunTimeId(id, ResolveState(id));
    }

    public static RunTimeId LiveRuntimeId(this IUIAutomationElement element)
    {
        int[] id;
        try
        {
            id = element.GetRuntimeId() ?? [];
        }
        catch
        {
            return new RunTimeId([], RunTimeIdStates.ErrorTryingGet);
        }

        return new RunTimeId(id, ResolveState(id));
    }

    private static RunTimeIdStates ResolveState(int[] id) =>
        id.Length == 0 ? RunTimeIdStates.Empty : RunTimeIdStates.Valid;
}
