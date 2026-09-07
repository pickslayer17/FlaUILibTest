using Interop.UIAutomationClient;
using UIDriver.CacheManagement;
using UIDriver.CustomModels;

public class UiNode
{
    public UiNode Parent;
    public UiNode[] Children;

    public CachedRunTimeId RunTimeId;
    public IUIAutomationElement Element;

    public int ControlType;
    public string Name;

    public NodeChangeState ChangeState = NodeChangeState.Original;
    public int? ChangedAtIteration;
}
