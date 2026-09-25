using UIDriver.Tree.Snapshots;

namespace UIDriver.Diagnostics.Remote;

public sealed record BranchSnapshotMessage(BranchSnapshot Snapshot) : SnapshotMessage;
