using UIDriver.Uia;
using UIDriver.Uia.Constants;

namespace UIDriver.Tree.Snapshots;

public sealed record NodeSnapshot(
    RunTimeId? RunTimeId,
    UiaControlType ControlType,
    string? Name,
    bool IsDirty,
    NodeChange Change,
    IReadOnlyList<NodeSnapshot> Children);
