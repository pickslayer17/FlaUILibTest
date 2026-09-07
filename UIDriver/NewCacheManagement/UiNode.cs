using Interop.UIAutomationClient;
using UIDriver.CustomModels;

namespace CacheManagement;

public class UiNode
{
    public UiNode Parent;
    public UiNode[] Children;

    public CachedRunTimeId RunTimeId;
    public IUIAutomationElement Element;

    public int ControlType;
    public string Name;
}
