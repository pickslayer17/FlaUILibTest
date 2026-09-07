using Interop.UIAutomationClient;
using UIDriver.CacheManagement;
using UIDriver.CustomModels;
namespace CacheManagement;

public class UICachedTree
{
    public UiNode Tree { get; }

    private readonly List<TreeSnapshot> _history = [];
    public IReadOnlyList<TreeSnapshot> History => _history;

    public UICachedTree(IUIAutomationElement cachedWindow)
    {
        Tree = BuildUINodeTree(cachedWindow);
    }

    public UiNode BuildUINodeTree(IUIAutomationElement element)
    {
        return BuildUINodeTree(element, null);
    }

    private UiNode BuildUINodeTree(IUIAutomationElement element, UiNode parent)
    {
        var node = BuildUINodeTreeCore(element, parent);

        return node;
    }

    private UiNode BuildUINodeTreeCore(IUIAutomationElement element, UiNode parent)
    {
        var runtimeId = element.CachedRuntimeId();
        var node = new UiNode(element)
        {
            Parent = parent,
            Element = element,
            RunTimeId = runtimeId,
        };

        var children = new List<UiNode>();
        var cachedChildren = element.GetCachedChildren();
        var childCount = cachedChildren?.Length ?? 0;

        for (var i = 0; i < childCount && cachedChildren != null; i++)
        {
            var childElement = cachedChildren.GetElement(i);
            children.Add(BuildUINodeTreeCore(childElement, node));
        }

        node.Children = children.ToArray();
        return node;
    }

    public void Add()
    {

    }

    public void Replace(UiNode target, UiNode branch, int iteration)
    {
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
