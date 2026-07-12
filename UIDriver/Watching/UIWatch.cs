using UIDriver.Matchers;

namespace UIDriver;

public enum WatchStatus
{
    Pending,
    Completed,
    Cancelled
}

public sealed class UIWatch
{
    public Guid Id { get; } = Guid.NewGuid();
    public WatchStatus Status { get; private set; } = WatchStatus.Pending;

    private Lock _findLock = new();
    private Lock _matchLock = new();
    private readonly IFinder _finder;
    private readonly IMatcher _matcher;
    private readonly TaskCompletionSource<UIAutomationElement> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public UIWatch(IFinder finder, IMatcher matcher)
    {
        _finder = finder;
        _matcher = matcher;
    }

    public Task<UIAutomationElement> Task => _tcs.Task;

    public bool TryResolveFindDescendant(UIAutomationElement source)
    {
        if (_tcs.Task.IsCompleted) return true;

        lock (_findLock)
        {
            var found = _finder.Find(source);
            if (found is null) return false;

            return Complete(found);
        }
    }

    public bool TryResolveMatch(UIAutomationElement source)
    {
        if (_tcs.Task.IsCompleted) return true;

        lock (_findLock)
        {
            if (_matcher.Matches(source))
            {
                Complete(source);
                return true;
            }

            return false;
        }
    }

    private bool Complete(UIAutomationElement foundElement)
    {
        var result = _tcs.TrySetResult(foundElement);
        Status = WatchStatus.Completed;
        return result;
    }

    public void Cancel()
    {
        _tcs.TrySetCanceled();
        Status = WatchStatus.Cancelled;
    }
}
