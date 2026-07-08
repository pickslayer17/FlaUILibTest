using FlaUI.Core.Definitions;
using FlaUI.Core.Identifiers;

public class UiNode
{
    public UiNode Parent;
    public UiNode[] Children;

    public ControlType ControlType;
    public string Name;
    public string ClassName;
    public string AutomationId;

    public object GetPropertyValue(PropertyId propertyId)
    {
        var propName = propertyId.Name;

        switch (propName)
        {
            case nameof(Name):
                return Name;

            case nameof(ClassName):
                return ClassName;

            case nameof(AutomationId):
                return AutomationId;

            case nameof(ControlType):
                return ControlType;   // или .ToString() если enum

            default:
                return null;   // или string.Empty
        }
        
    }
}
