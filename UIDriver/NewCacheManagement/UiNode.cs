using Interop.UIAutomationClient;
using UIDriver.CustomModels;

namespace CacheManagement;

public class UiNode
{
    public UiNode Parent { get; set; }
    public UiNode[] Children { get; set; }
    public IUIAutomationElement Element { get; init; }
    public CachedRunTimeId RunTimeId { get; set; }

    public UiNode(IUIAutomationElement element)
    {
        Element = element;
    }
}
