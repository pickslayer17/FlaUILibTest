using UIDriver.Diagnostics;
using UIDriver.Exceptions;
using UIDriver.Tree.Snapshots;
using UIDriver.Uia;

namespace UIDriver.Tree;

public sealed class UICachedTree
{
    private readonly RunTimeId _windowRunTimeId;
    private readonly string? _windowTitle;
    private readonly SnapshotPublisher _snapshotPublisher;
    private readonly NodeChangeHistory _changeHistory = new();

    public UiNode Tree { get; private set; }
    public int Version { get; private set; }

    public UICachedTree(UiaElement cachedWindow, RunTimeId windowRunTimeId, string? windowTitle, SnapshotPublisher snapshotPublisher)
    {
        _windowRunTimeId = windowRunTimeId;
        _windowTitle = windowTitle;
        _snapshotPublisher = snapshotPublisher;
        Tree = BranchFactory.BuildUINodeTree(cachedWindow);
    }

    public void Add(HeeledBranch branch)
    {
        var parent = FindExistingNode(branch.Heel);
        branch.Tree.Parent = parent;
        LinkChildToParent(branch.Tree, parent);

        PublishChange(NodeChangeState.Added, branch.Tree);
    }

    public void Replace(Branch branch)
    {
        var target = FindExistingNode(branch.Tree);
        ReplaceNode(target, branch.Tree);

        PublishChange(NodeChangeState.Replaced, branch.Tree);
    }

    public void MarkDirty(Branch branch)
    {
        var target = FindExistingNode(branch.Tree);
        target.IsDirty = true;

        PublishChange(NodeChangeState.Dirty, target);
    }

    public void PublishSnapshot()
    {
        var root = NodeSnapshotFactory.Create(Tree, _changeHistory);
        _snapshotPublisher.PublishTree(new TreeSnapshot(_windowRunTimeId, _windowTitle, Version, DateTime.Now, root));
    }

    private void PublishChange(NodeChangeState state, UiNode changedBranch)
    {
        Version++;
        _changeHistory.Record(changedBranch, state, Version);
        PublishSnapshot();
    }

    private UiNode FindExistingNode(UiNode node)
    {
        var runTimeId = node.RunTimeId
            ?? throw new CachedTreeInconsistencyException("Node without RuntimeId cannot be located in the cached tree.");

        return FindNode(Tree, candidate => runTimeId.Equals(candidate.RunTimeId))
            ?? throw new CachedTreeInconsistencyException($"Node [{runTimeId}] not found in the cached tree.");
    }

    private void ReplaceNode(UiNode target, UiNode replacement)
    {
        var parent = target.Parent;
        if (parent == null)
        {
            Tree = replacement;
            return;
        }

        replacement.Parent = parent;
        parent.Children = parent.Children.Select(child => child == target ? replacement : child).ToArray();
        target.Parent = null;
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
