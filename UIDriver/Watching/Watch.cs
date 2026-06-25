namespace UIDriver;

public enum WatchStatus
{
    Pending,
    Completed,
    Cancelled
}

public sealed class Watch
{
    public Guid Id { get; } = Guid.NewGuid();
    public WatchStatus Status { get; private set; } = WatchStatus.Pending;

    private readonly IFinder _finder;
    private readonly TaskCompletionSource<AutomationElementObject> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Watch(IFinder finder)
    {
        _finder = finder;
    }

    public Task<AutomationElementObject> Task => _tcs.Task;

    public bool TryResolveFindDescendant(AutomationElementObject source)
    {
        if (_tcs.Task.IsCompleted) return true;

        var found = _finder.Find(source);
        if (found is null) return false;

        return Complete(found);
    }

    public bool TryResolveMatch(AutomationElementObject source)
    {
        if (_tcs.Task.IsCompleted) return true;
        if (!_finder.Matches(source)) return false;

        return Complete(source);
    }

    private bool Complete(AutomationElementObject result)
    {
        Status = WatchStatus.Completed;
        return _tcs.TrySetResult(result);
    }

    public void Cancel()
    {
        Status = WatchStatus.Cancelled;
        _tcs.TrySetCanceled();
    }
}
