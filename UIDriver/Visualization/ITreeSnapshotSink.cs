using UIDriver.CacheManagement;

namespace UIDriver.Visualization;

public interface ITreeSnapshotSink
{
    void OnSnapshot(object owner, string title, TreeSnapshot snapshot);
}
