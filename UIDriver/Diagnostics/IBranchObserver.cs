using UIDriver.Tree.Snapshots;

namespace UIDriver.Diagnostics;

public interface IBranchObserver
{
    public void OnBranchSnapshot(BranchSnapshot snapshot);
}
