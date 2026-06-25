using System.Windows.Forms;

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

    private Lock _resolveLocker = new();
    private readonly IFinder _finder;
    private readonly TaskCompletionSource<AutomationElementObject> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Watch(IFinder finder)
    {
        _finder = finder;
    }

    public Task<AutomationElementObject> Task => _tcs.Task;

    public bool TryResolveFindDescendant(AutomationElementObject source)
    {
        LogEventFactory.RaiseText($"Trying to resolve Descendants with runtimeId: {source.RunTimeId}");
        if (_tcs.Task.IsCompleted) return true;
        if (_finder.Matches(source)) Complete(source);

        var found = _finder.Find(source);
        if (found is null) return false;

        return Complete(found);
    }

    public bool TryResolveMatch(AutomationElementObject source)
    {
        LogEventFactory.RaiseText($"Trying to resolve match for element with runtimeId: {source.RunTimeId}");
        if (_tcs.Task.IsCompleted) return true;
        if (_finder.Matches(source)) Complete(source);

        return false;
    }

    private bool Complete(AutomationElementObject foundElement)
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
