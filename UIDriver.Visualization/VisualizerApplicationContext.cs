using UIDriver.Diagnostics.Remote;

namespace UIDriver.Visualization;

internal sealed class VisualizerApplicationContext : ApplicationContext
{
    private readonly TreeVisualizerForm _treeVisualizerForm = new();
    private readonly BranchVisualizerForm _branchVisualizerForm = new();

    public VisualizerApplicationContext()
    {
        ShowForm(_treeVisualizerForm);
        ShowForm(_branchVisualizerForm);
        new SnapshotPipeServer(StartNewSession, Render).Start();
    }

    private void ShowForm(Form form)
    {
        form.FormClosed += (_, _) => ExitThread();
        form.Show();
    }

    private void StartNewSession()
    {
        _treeVisualizerForm.ClearAll();
        _branchVisualizerForm.ClearAll();
    }

    private void Render(SnapshotMessage message)
    {
        switch (message)
        {
            case TreeSnapshotMessage treeSnapshotMessage:
                _treeVisualizerForm.RenderSnapshot(treeSnapshotMessage.Snapshot);
                break;
            case BranchSnapshotMessage branchSnapshotMessage:
                _branchVisualizerForm.AddBranch(branchSnapshotMessage.Snapshot);
                break;
            default:
                throw new NotImplementedException($"No renderer for {message.GetType().Name}.");
        }
    }
}
