using FlaUI.Core.AutomationElements;

namespace FlaUILibTest.Extensions;

public static class AutomationElementExtensions
{
    public static bool TryGetWindowRunTimeId(this AutomationElement window, out int[] runTimeId)
    {
        try
        {
            runTimeId = window.Properties.RuntimeId;
        }
        catch
        {
            runTimeId = null;
            return false;
        }

        return true;
    }
}
