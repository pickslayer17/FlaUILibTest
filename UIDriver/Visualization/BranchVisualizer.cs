using System.Windows.Forms;

namespace UIDriver.Visualization;

public sealed class BranchVisualizer
{
    public static BranchVisualizer Instance { get; } = new();

    private BranchVisualizerForm? _form;
    private readonly object _lock = new();

    private BranchVisualizer() { }

    public void AddBranch(string title, NodeSnapshot branch)
    {
        EnsureStarted();
        _form!.AddBranch(title, branch);
    }

    private void EnsureStarted()
    {
        lock (_lock)
        {
            if (_form != null) return;

            var ready = new ManualResetEventSlim();
            var thread = new Thread(() =>
            {
                _form = new BranchVisualizerForm();
                _form.Load += (_, _) => ready.Set();
                Application.Run(_form);
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
            ready.Wait();
        }
    }
}
