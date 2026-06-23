using FlaUI.Core.AutomationElements;

namespace UIDriver;

public interface IFinder
{
    AutomationElement? Find(AutomationElement source);
    AutomationElement[] FindAll(AutomationElement source);
}
