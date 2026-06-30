using FlaUI.Core.Definitions;

class UiNode
{
    public UiNode Parent;
    public UiNode[] Children;

    public ControlType ControlType;
    public string Name;
    public string ClassName;
    public string AutomationId;
}
