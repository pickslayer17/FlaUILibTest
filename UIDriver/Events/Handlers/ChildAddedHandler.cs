using Microsoft.Extensions.Logging;
using UIDriver.Diagnostics;
using UIDriver.Exceptions;
using UIDriver.Tree;

namespace UIDriver.Events.Handlers;

public sealed class ChildAddedHandler
{
    private readonly SnapshotPublisher _snapshotPublisher;
    private readonly ILogger<ChildAddedHandler> _logger;

    public ChildAddedHandler(SnapshotPublisher snapshotPublisher, ILogger<ChildAddedHandler> logger)
    {
        _snapshotPublisher = snapshotPublisher;
        _logger = logger;
    }

    public void Handle(StructureChangedEvent structureChanged)
    {
        var addedChild = structureChanged.Source;
        var addedChildRunTimeId = addedChild.Cached.RunTimeId;
        if (addedChildRunTimeId is null)
        {
            _logger.LogDebug("ADDED: source has no RuntimeId, skip");
            return;
        }

        var liveParent = addedChild.Live.Parent
            ?? throw new CachedTreeInconsistencyException($"ADDED: source [{addedChildRunTimeId}] has no live parent.");

        var branch = BranchFactory.BuildHeeledBranch(addedChild, liveParent);
        _snapshotPublisher.PublishBranch(branch, structureChanged.ChangeType);

        _logger.LogDebug("ADDED: [{Child}] to heel [{Heel}]", addedChildRunTimeId, branch.Heel.RunTimeId);
        structureChanged.CachedTree.Add(branch);
    }
}
