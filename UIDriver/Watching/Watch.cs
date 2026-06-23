using FlaUI.Core.AutomationElements;

namespace UIDriver;

public enum WatchStatus
{
    Pending,
    Completed,
    TimedOut,
    Cancelled
}

public sealed class Watch
{
    public IFinder Finder { get; }
    public AutomationElement Source { get; }
    public TimeSpan Timeout { get; }
    public WatchStatus Status { get; private set; } = WatchStatus.Pending;

    public Watch(IFinder finder, AutomationElement source, TimeSpan timeout)
    {
        Finder = finder;
        Source = source;
        Timeout = timeout;
    }

    public Task<AutomationElement> Task => throw new NotImplementedException();

    public bool TryResolve() => throw new NotImplementedException();
}
