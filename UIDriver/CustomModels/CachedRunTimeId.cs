using Interop.UIAutomationClient;

namespace UIDriver.CustomModels;

public class CachedRunTimeId : RunTimeId
{
    public CachedRunTimeId(int[] id) : base(id)
    {
    }

    public CachedRunTimeId(IUIAutomationElement element) : base(element)
    {
    }

    protected override int[] GetRunTimeId(IUIAutomationElement element)
    {
        try
        {
            if (element.GetCachedPropertyValue((int)UiaProperty.RuntimeId) is int[] cached)
                return cached;

            return [];
        }
        catch (ArgumentException)
        {
            return [0xA, 0xA, 0xA, 0xA, 0xA, 0xA];
        }
    }
}
