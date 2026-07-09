using Interop.UIAutomationClient;

public sealed class FocusChangedHandler : IUIAutomationFocusChangedEventHandler
{
    const int UIA_NamePropertyId = 30005;
    const int UIA_ControlTypePropertyId = 30003;

    public void HandleFocusChangedEvent(IUIAutomationElement sender)
    {
        var name = sender.GetCurrentPropertyValue(UIA_NamePropertyId);
        var controlType = sender.GetCurrentPropertyValue(UIA_ControlTypePropertyId);
        Console.WriteLine($"[FOCUS] controlType={controlType} name='{name}'");
    }
}
