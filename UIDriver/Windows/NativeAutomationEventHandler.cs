using Interop.UIAutomationClient;

namespace UIDriver;

public sealed class NativeAutomationEventHandler : IUIAutomationEventHandler
{
    private readonly Action<IUIAutomationElement, int> _onEvent;

    public NativeAutomationEventHandler(Action<IUIAutomationElement, int> onEvent)
    {
        _onEvent = onEvent;
    }

    public void HandleAutomationEvent(IUIAutomationElement sender, int eventId)
    {
        _onEvent(sender, eventId);
    }
}
