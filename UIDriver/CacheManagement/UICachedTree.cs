using Interop.UIAutomationClient;
using UIDriver;
using UIDriver.Constants;
using UIDriver.CustomModels;

public class UICachedTree
{
    public UiNode Tree { get; }
    public Dictionary<RunTimeId, UiNode> NodesByRunTimeId { get; } = new();

    public UICachedTree(IUIAutomationElement cachedWindow)
    {
        Tree = BuildUINodeTree(cachedWindow, null);
    }

    public UiNode BuildUINodeTree(IUIAutomationElement element, UiNode parent)
    {
        var node = new UiNode
        {
            Parent = parent,
            Element = element,
            RunTimeId = SafeRunTimeId(element),
            ControlType = SafeInt(element, (int)UiaProperty.ControlType),
            Name = SafeString(element, (int)UiaProperty.Name)
        };

        if (!NodesByRunTimeId.TryAdd(node.RunTimeId, node))
            throw new InvalidOperationException($"Duplicate RuntimeId in cached tree: {node.RunTimeId}");

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

    public void UpdateNode(UiNode oldNode, IUIAutomationElement updatedElement)
    {
        var parent = oldNode.Parent;
        RemoveSubtree(oldNode);

        var newSubTree = BuildUINodeTree(updatedElement, parent);

        LinkChildToParent(newSubTree, parent);
    }

    private void RemoveSubtree(UiNode node)
    {
        UnlinkChildFromParent(node, node.Parent);

        node.Parent = null;
        NodesByRunTimeId.Remove(node.RunTimeId);

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
