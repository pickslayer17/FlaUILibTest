using UIDriver.Uia;

namespace UIDriver.Events;

public sealed record WindowClosedEvent(RunTimeId WindowRunTimeId) : DriverEvent;
