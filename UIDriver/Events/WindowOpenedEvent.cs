using UIDriver.Uia;

namespace UIDriver.Events;

public sealed record WindowOpenedEvent(UiaElement Window) : DriverEvent;
