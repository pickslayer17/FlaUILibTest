using Interop.UIAutomationClient;

namespace UIDriver;

public sealed class NativePropertyChangedHandler : IUIAutomationPropertyChangedEventHandler
{
    private readonly Action<IUIAutomationElement, int, object> _onPropertyChanged;

    public NativePropertyChangedHandler(Action<IUIAutomationElement, int, object> onPropertyChanged)
    {
        _onPropertyChanged = onPropertyChanged;
    }

    public void HandlePropertyChangedEvent(IUIAutomationElement sender, int propertyId, object newValue)
    {
        _onPropertyChanged(sender, propertyId, newValue);
    }
}
