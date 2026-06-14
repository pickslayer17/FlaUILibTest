using FlaUI.Core.AutomationElements;

namespace FlaUILibTest.Inspector;

public class TreeNode
{
    public AutomationElement Element { get; }
    public TreeNode? Parent { get; }
    public List<TreeNode> Children { get; } = new();

    public string Name => TryGet(() => Element.Properties.Name.ValueOrDefault ?? "");
    public string ClassName => TryGet(() => Element.Properties.ClassName.ValueOrDefault ?? "");
    public string ControlType => TryGet(() => Element.Properties.ControlType.ValueOrDefault.ToString() ?? "");
    public string AutomationId => TryGet(() => Element.Properties.AutomationId.ValueOrDefault ?? "");
    public int Depth { get; }

    public TreeNode(AutomationElement element, TreeNode? parent, int depth)
    {
        Element = element;
        Parent = parent;
        Depth = depth;
    }

    public void AddChild(TreeNode child)
    {
        Children.Add(child);
    }

    private static string TryGet(Func<string> getter)
    {
        try { return getter(); }
        catch { return "<dead>"; }
    }
}