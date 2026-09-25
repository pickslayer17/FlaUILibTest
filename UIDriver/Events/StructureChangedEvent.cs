using UIDriver.Tree;
using UIDriver.Uia;
using UIDriver.Uia.Constants;

namespace UIDriver.Events;

public sealed record StructureChangedEvent(
    UICachedTree CachedTree,
    UiaElement Source,
    UiaStructureChangeType ChangeType,
    RunTimeId? TargetRunTimeId) : DriverEvent;
