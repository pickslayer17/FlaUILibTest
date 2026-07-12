namespace UIDriver.Interfaces;

public interface IPropertyChangedListener
{
    public void NotifyOnPropertyChanged(UIAutomationElement source);
}
