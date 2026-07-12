using Interop.UIAutomationClient;
using UIDriver.CustomModels;

namespace UIDriver;

public sealed class UIAutomationElement
{
    public IUIAutomationElement Element { get; }

    public RunTimeId RunTimeId => new(Element);

    public UIAutomationElement(IUIAutomationElement element) => Element = element;
}
