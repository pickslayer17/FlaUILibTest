using UIDriver.Uia;

namespace UIDriver.Tree.Snapshots;

public sealed record TreeSnapshot(
    RunTimeId WindowRunTimeId,
    string? WindowTitle,
    int Version,
    DateTime TakenAt,
    NodeSnapshot Root);
