namespace UIDriver;

public abstract record LogEvent;

public sealed record OrderCreated(Guid OrderId, BY By) : LogEvent;

public sealed record ElementResolved(Guid OrderId) : LogEvent;

public sealed record WindowOpened(RunTimeId Window) : LogEvent;

public sealed record WindowClosed(RunTimeId Window) : LogEvent;
