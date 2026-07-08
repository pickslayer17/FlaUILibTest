using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using FlaUILibTest.Extensions;

public static class Helpers
{
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

    public static CachedNode BuildCachedNode(AutomationElement element, CachedNode? parent)
    {
        var node = new CachedNode
        {
            Parent = parent,
            Name = SafeName(element),
            ProcessId = SafeProcessId(element),
            RuntimeId = SafeRunTimeId(element)
        };

        foreach (var child in element.CachedChildren)
            node.Children.Add(BuildCachedNode(child, node));

        return node;
    }

    public static int PrintCachedTreeSteps;

    public static void PrintCachedTree(AutomationElement element, int depth, System.Text.StringBuilder output)
    {
        PrintCachedTreeSteps++;
        var name = SafeName(element);
        var processId = SafeProcessId(element);
        var runtimeId = SafeRunTimeId(element).ToFormattedString();

        output.AppendLine($"{new string(' ', depth * 2)}name='{name}' pid={processId} runtimeId={runtimeId}");

        foreach (var child in element.CachedChildren)
            PrintCachedTree(child, depth + 1, output);
    }

    public static void PrintUiNodeTree(UiNode node, int depth)
    {
        if (node == null) return;

        var marker = node.ClassName == "target" ? "  <<<<< TARGET" : "";
        Console.WriteLine($"{new string(' ', depth * 2)}[{node.ControlType}] name='{node.Name}' automationId='{node.AutomationId}' className='{node.ClassName}'{marker}");

        foreach (var child in node.Children ?? [])
            PrintUiNodeTree(child, depth + 1);
    }
}
