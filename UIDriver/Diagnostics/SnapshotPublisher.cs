using UIDriver.Tree;
using UIDriver.Tree.Snapshots;
using UIDriver.Uia.Constants;

namespace UIDriver.Diagnostics;

public sealed class SnapshotPublisher
{
    private readonly IReadOnlyList<ITreeObserver> _treeObservers;
    private readonly IReadOnlyList<IBranchObserver> _branchObservers;
    private int _branchCount;

    public SnapshotPublisher(IReadOnlyList<ITreeObserver> treeObservers, IReadOnlyList<IBranchObserver> branchObservers)
    {
        _treeObservers = treeObservers;
        _branchObservers = branchObservers;
    }

    public void PublishTree(TreeSnapshot snapshot)
    {
        foreach (var treeObserver in _treeObservers)
            treeObserver.OnTreeSnapshot(snapshot);
    }

    public void PublishBranch(Branch branch, UiaStructureChangeType origin)
    {
        var root = NodeSnapshotFactory.Create(branch.Tree, new NodeChangeHistory());
        var snapshot = new BranchSnapshot(++_branchCount, origin, branch.Top.RunTimeId, root);

        foreach (var branchObserver in _branchObservers)
            branchObserver.OnBranchSnapshot(snapshot);
    }
}
