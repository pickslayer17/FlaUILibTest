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
    private readonly AutomationElementObject _source;
    private readonly TaskCompletionSource<AutomationElementObject> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Watch(IFinder finder, AutomationElementObject source)
    {
        _finder = finder;
        _source = source;
    }

    public Task<AutomationElementObject> Task => _tcs.Task;

    public bool TryResolve()
    {
        if (_tcs.Task.IsCompleted) return true;

        var found = _finder.Find(_source);
        if (found is null) return false;

        Status = WatchStatus.Completed;
        return _tcs.TrySetResult(found);
    }

    public void Cancel()
    {
        Status = WatchStatus.Cancelled;
        _tcs.TrySetCanceled();
    }
}
