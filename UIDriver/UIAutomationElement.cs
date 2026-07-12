using FlaUI.Core.AutomationElements;
using UIDriver.CustomModels;

namespace UIDriver;

public sealed class UIAutomationElement
{
    public AutomationElement Element { get; }

    public RunTimeId RunTimeId => new(Element);

    public UIAutomationElement(AutomationElement element) => Element = element;
}
