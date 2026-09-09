using Interop.UIAutomationClient;
using UIDriver.NewCacheManagement;
namespace CacheManagement;

public class UICachedTree
{
    public UiNode Tree { get; }

    public UICachedTree(IUIAutomationElement cachedWindow)
    {
        Tree = BranchFactory.BuildUINodeTree(cachedWindow);
    }

    public void Add(UiNode nodeToAdd)
    {

    }

    public void Remove(UiNode nodeToRemove)
    {

    }

    public void Replace(UiNode target, UiNode branch)
    {
    }

    private static UiNode? FindNode(UiNode node, Func<UiNode, bool> condition)
        => node?.Traverse().FirstOrDefault(condition);

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
}
