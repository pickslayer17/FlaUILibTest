using UIDriver.Diagnostics;
using UIDriver.Tree.Snapshots;

namespace UIDriver.Visualization;

public sealed class TreeVisualizer : ITreeObserver
{
    public static TreeVisualizer Instance { get; } = new();

    private readonly StaFormHost<TreeVisualizerForm> _formHost = new();

    private TreeVisualizer() { }

    public void OnTreeSnapshot(TreeSnapshot snapshot) => _formHost.StartedForm.RenderSnapshot(snapshot);
}
