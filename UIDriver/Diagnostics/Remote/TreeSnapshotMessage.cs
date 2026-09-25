using UIDriver.Tree.Snapshots;

namespace UIDriver.Diagnostics.Remote;

public sealed record TreeSnapshotMessage(TreeSnapshot Snapshot) : SnapshotMessage;
