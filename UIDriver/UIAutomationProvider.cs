using Interop.UIAutomationClient;

namespace UIDriver;

public static class UIAutomationProvider
{
    public static IUIAutomation Automation { get; set; } = null!;
}
