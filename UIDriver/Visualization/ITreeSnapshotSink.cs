using UIDriver.CacheManagement;

namespace UIDriver.Visualization;

public interface ITreeSnapshotSink
{
    void OnSnapshot(ContainerId container, string title, TreeSnapshot snapshot);
}
