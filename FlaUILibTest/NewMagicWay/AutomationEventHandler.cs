using Interop.UIAutomationClient;

public sealed class AutomationEventHandler : IUIAutomationEventHandler
{
    const int UIA_NamePropertyId = 30005;
    const int UIA_ControlTypePropertyId = 30003;

    public void HandleAutomationEvent(IUIAutomationElement sender, int eventId)
    {
        var name = sender.GetCurrentPropertyValue(UIA_NamePropertyId);
        var controlType = sender.GetCurrentPropertyValue(UIA_ControlTypePropertyId);
        Console.WriteLine($"[EVENT {eventId}] controlType={controlType} name='{name}'");
    }
}
