using UIDriver.Tree;
using UIDriver.Uia;
using UIDriver.Uia.Constants;

namespace UIDriver.Events;

public sealed record PropertyChangedEvent(
    UICachedTree CachedTree,
    UiaElement Source,
    UiaProperty Property,
    object NewValue) : DriverEvent;
