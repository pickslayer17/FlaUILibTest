using Interop.UIAutomationClient;

namespace UIDriver.Interfaces;

public interface IPropertyChangedListener
{
    public void NotifyOnPropertyChanged(IUIAutomationElement source, int propertyId, object newValue);
}
