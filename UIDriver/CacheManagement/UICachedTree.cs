using FlaUI.Core.AutomationElements;
using Interop.UIAutomationClient;

public class UICachedTree
{
    public IUIAutomationElement CachedWindow { get; }

    public UICachedTree(IUIAutomationElement cachedWindow)
    {
        CachedWindow = cachedWindow;
    }

    public static UiNode BuildUINodeTree(AutomationElement element, UiNode parent)
    {
        var node = new UiNode
        {
            Parent = parent,
            ControlType = element.ControlType,
            Name = SafeName(element)?.ToString(),
            ClassName = SafeClassName(element),
            AutomationId = SafeAutomationId(element)
        };

        var children = new List<UiNode>();
        foreach (var child in element.CachedChildren)
            children.Add(BuildUINodeTree(child, node));

        node.Children = children.ToArray();
        return node;
    }

    public static int[] SafeRunTimeId(AutomationElement element)
    {
        try { return element.Properties.RuntimeId.Value; }
        catch { return new int[0]; }
    }

    public static int SafeProcessId(AutomationElement element)
    {
        try { return element.Properties.ProcessId.Value; }
        catch (Exception ex) { throw new Exception("oppa nihuya sebe! element bez ProcessId", ex); }
    }

    public static object? SafeName(AutomationElement element)
    {
        try { return element.Name; }
        catch { return null; }
    }

    public static string SafeClassName(AutomationElement element)
    {
        try { return element.ClassName; }
        catch { return null; }
    }

    public static string SafeAutomationId(AutomationElement element)
    {
        try { return element.AutomationId; }
        catch { return null; }
    }
}