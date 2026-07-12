public class UiNode
{
    public UiNode Parent;
    public UiNode[] Children;

    public int ControlType;
    public string Name;
    public string ClassName;
    public string AutomationId;

    public object GetPropertyValue(int propertyId)
    {
        switch (propertyId)
        {
            case (int)UIDriver.UiaProperty.Name:
                return Name;

            case (int)UIDriver.UiaProperty.ClassName:
                return ClassName;

            case (int)UIDriver.UiaProperty.AutomationId:
                return AutomationId;

            case (int)UIDriver.UiaProperty.ControlType:
                return ControlType;

            default:
                return null;
        }
    }
}
