using UIDriver.Uia;
using UIDriver.Uia.Constants;

namespace UIDriver.Tree.Snapshots;

public sealed record BranchSnapshot(
    int Number,
    UiaStructureChangeType Origin,
    RunTimeId? TopRunTimeId,
    NodeSnapshot Root);
