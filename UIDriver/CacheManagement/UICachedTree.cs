using Interop.UIAutomationClient;
using UIDriver;
using UIDriver.CustomModels;

public class UICachedTree
{
    public UiNode Tree { get; }

    public UICachedTree(IUIAutomationElement cachedWindow)
    {
        Tree = BuildUINodeTree(cachedWindow);
    }

    private int _nodeCount;
    private long _runtimeIdTicks;
    private long _controlTypeTicks;
    private long _nameTicks;
    private long _getChildrenTicks;
    private long _getElementTicks;

    public UiNode BuildUINodeTree(IUIAutomationElement element)
    {
        return BuildUINodeTree(element, null);
    }

    private UiNode BuildUINodeTree(IUIAutomationElement element, UiNode parent)
    {
        _nodeCount = 0;
        _runtimeIdTicks = 0;
        _controlTypeTicks = 0;
        _nameTicks = 0;
        _getChildrenTicks = 0;
        _getElementTicks = 0;

        var node = BuildUINodeTreeCore(element, parent);

        return node;
    }

    private UiNode BuildUINodeTreeCore(IUIAutomationElement element, UiNode parent)
    {
        _nodeCount++;

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var runtimeId = SafeRunTimeId(element);
        _runtimeIdTicks += sw.ElapsedTicks;

        sw.Restart();
        var controlType = SafeInt(element, (int)UiaProperty.ControlType);
        _controlTypeTicks += sw.ElapsedTicks;

        sw.Restart();
        var name = SafeString(element, (int)UiaProperty.Name);
        _nameTicks += sw.ElapsedTicks;

        var node = new UiNode
        {
            Parent = parent,
            Element = element,
            RunTimeId = runtimeId,
            ControlType = controlType,
            Name = name
        };

        var children = new List<UiNode>();

        sw.Restart();
        var cachedChildren = element.GetCachedChildren();
        var childCount = cachedChildren?.Length ?? 0;
        _getChildrenTicks += sw.ElapsedTicks;

        for (var i = 0; i < childCount && cachedChildren != null; i++)
        {
            sw.Restart();
            var childElement = cachedChildren.GetElement(i);
            _getElementTicks += sw.ElapsedTicks;

            children.Add(BuildUINodeTreeCore(childElement, node));
        }

        node.Children = children.ToArray();
        return node;
    }

    private static double TicksToMs(long ticks) =>
        Math.Round(ticks * 1000.0 / System.Diagnostics.Stopwatch.Frequency, 1);

    public void Remove(UiNode node)
    {
        RemoveSubtree(node);
    }

    public void AddNode(UiNode parent, UiNode node)
    {
        node.Parent = parent;
        LinkChildToParent(node, parent);
    }

    public UiNode? GetNode(Func<UiNode, bool> condition)
    {
        return FindNode(Tree, condition);
    }

    private static UiNode? FindNode(UiNode node, Func<UiNode, bool> condition)
    {
        if (node == null) return null;
        if (condition(node)) return node;

        foreach (var child in node.Children ?? [])
        {
            var match = FindNode(child, condition);
            if (match != null) return match;
        }

        return null;
    }

    private void RemoveSubtree(UiNode node)
    {
        UnlinkChildFromParent(node, node.Parent);
        node.Parent = null;

        foreach (var child in node.Children ?? [])
            RemoveSubtree(child);
    }

    private static void LinkChildToParent(UiNode child, UiNode parent)
    {
        if (parent == null) return;

        var oldChildren = parent.Children ?? [];
        var newChildren = new UiNode[oldChildren.Length + 1];

        for (var i = 0; i < oldChildren.Length; i++)
            newChildren[i] = oldChildren[i];
        newChildren[oldChildren.Length] = child;

        parent.Children = newChildren;
    }

    private static void UnlinkChildFromParent(UiNode child, UiNode parent)
    {
        if (parent == null) return;
        if (parent.Children == null) return;

        var oldChildren = parent.Children;
        var newChildren = new UiNode[oldChildren.Length - 1];

        var writeIndex = 0;
        for (var readIndex = 0; readIndex < oldChildren.Length; readIndex++)
        {
            if (oldChildren[readIndex] == child) continue;
            newChildren[writeIndex] = oldChildren[readIndex];
            writeIndex++;
        }

        parent.Children = newChildren;
    }

    private static RunTimeId SafeRunTimeId(IUIAutomationElement element)
    {
        try { return new RunTimeId(element.GetCachedPropertyValue((int)UiaProperty.RuntimeId) as int[] ?? []); }
        catch { return new RunTimeId([]); }
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
