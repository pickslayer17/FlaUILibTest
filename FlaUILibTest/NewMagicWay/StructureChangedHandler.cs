using Interop.UIAutomationClient;

public sealed class StructureChangedHandler : IUIAutomationStructureChangedEventHandler
{
    const int UIA_NamePropertyId = 30005;
    const int UIA_ControlTypePropertyId = 30003;

    public void HandleStructureChangedEvent(IUIAutomationElement sender, StructureChangeType changeType, int[] runtimeId)
    {
        object name = null;
        object controlType = null;
        try
        {
            name = sender.GetCurrentPropertyValue(UIA_NamePropertyId);
            controlType = sender.GetCurrentPropertyValue(UIA_ControlTypePropertyId);
        }
        catch { }

        var runtimeIdText = runtimeId == null ? "" : string.Join(".", runtimeId);
        Console.WriteLine($"[STRUCTURE {changeType}] controlType={controlType} name='{name}' runtimeId={runtimeIdText}");
    }
}
