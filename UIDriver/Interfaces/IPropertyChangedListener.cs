namespace UIDriver.Interfaces;

public interface IPropertyChangedListener
{
    public void NotifyOnPropertyChanged(UIAutomationElement source, int propertyId, object newValue);
}
