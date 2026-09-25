using Microsoft.Extensions.Logging;
using UIDriver.Diagnostics;
using UIDriver.Exceptions;
using UIDriver.Tree;

namespace UIDriver.Events.Handlers;

public sealed class ChildrenInvalidatedHandler
{
    private readonly SnapshotPublisher _snapshotPublisher;
    private readonly ILogger<ChildrenInvalidatedHandler> _logger;

    public ChildrenInvalidatedHandler(SnapshotPublisher snapshotPublisher, ILogger<ChildrenInvalidatedHandler> logger)
    {
        _snapshotPublisher = snapshotPublisher;
        _logger = logger;
    }

    public void Handle(StructureChangedEvent structureChanged)
    {
        var invalidatedParent = structureChanged.Source;
        if (invalidatedParent.Cached.RunTimeId is null)
        {
            _logger.LogDebug("INVALIDATED: source has no RuntimeId, skip");
            return;
        }

        var branch = BranchFactory.BuildBranch(invalidatedParent);
        _snapshotPublisher.PublishBranch(branch, structureChanged.ChangeType);

        ApplyToTree(branch, structureChanged.CachedTree);
    }

    private void ApplyToTree(Branch branch, UICachedTree cachedTree)
    {
        if (branch.AllDescendantsHaveRunTimeId())
        {
            _logger.LogDebug("INVALIDATED: replace [{Branch}]", branch.Tree.RunTimeId);
            cachedTree.Replace(branch);
            return;
        }

        if (branch.NoDescendantHasRunTimeId())
        {
            _logger.LogDebug("INVALIDATED: mark dirty [{Branch}]", branch.Tree.RunTimeId);
            cachedTree.MarkDirty(branch);
            return;
        }

        throw new CachedTreeInconsistencyException($"INVALIDATED: branch [{branch.Tree.RunTimeId}] mixes descendants with and without RuntimeId.");
    }
}
