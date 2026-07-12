using Interop.UIAutomationClient;
using UIDriver;
using UIDriver.Constants;

public class UICachedTree
{
    public IUIAutomationElement CachedWindow { get; }

    public UICachedTree(IUIAutomationElement cachedWindow)
    {
        CachedWindow = cachedWindow;
    }

    public static UiNode BuildUINodeTree(IUIAutomationElement element, UiNode parent)
    {
        var node = new UiNode
        {
            Parent = parent,
            ControlType = SafeInt(element, (int)UiaProperty.ControlType),
            Name = SafeString(element, (int)UiaProperty.Name),
            ClassName = SafeString(element, (int)UiaProperty.ClassName),
            AutomationId = SafeString(element, (int)UiaProperty.AutomationId)
        };

        var children = new List<UiNode>();
        var cachedChildren = element.GetCachedChildren();
        if (cachedChildren != null)
        {
            for (var i = 0; i < cachedChildren.Length; i++)
                children.Add(BuildUINodeTree(cachedChildren.GetElement(i), node));
        }

        node.Children = children.ToArray();
        return node;
    }

    public static int[] SafeRunTimeId(IUIAutomationElement element)
    {
        try { return (int[])element.GetRuntimeId(); }
        catch { return new int[0]; }
    }

    private static string SafeString(IUIAutomationElement element, int propertyId)
    {
        try { return element.GetCachedPropertyValue(propertyId) as string; }
        catch { return null; }
    }

    private static int SafeInt(IUIAutomationElement element, int propertyId)
    {
        try { return Convert.ToInt32(element.GetCachedPropertyValue(propertyId)); }
        catch { return 0; }
    }
}
