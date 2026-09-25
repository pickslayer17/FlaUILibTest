using UIDriver.Diagnostics;
using UIDriver.Tree.Snapshots;

namespace UIDriver.Visualization;

public sealed class BranchVisualizer : IBranchObserver
{
    public static BranchVisualizer Instance { get; } = new();

    private readonly StaFormHost<BranchVisualizerForm> _formHost = new();

    private BranchVisualizer() { }

    public void OnBranchSnapshot(BranchSnapshot snapshot) => _formHost.StartedForm.AddBranch(snapshot);
}
