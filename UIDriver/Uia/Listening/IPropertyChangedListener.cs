using UIDriver.Uia.Constants;

namespace UIDriver.Uia.Listening;

public interface IPropertyChangedListener
{
    public void NotifyOnPropertyChanged(UiaElement source, UiaProperty property, object newValue);
}
