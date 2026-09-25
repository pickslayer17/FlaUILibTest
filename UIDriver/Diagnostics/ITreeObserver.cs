using UIDriver.Tree.Snapshots;

namespace UIDriver.Diagnostics;

public interface ITreeObserver
{
    public void OnTreeSnapshot(TreeSnapshot snapshot);
}
