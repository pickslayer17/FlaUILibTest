using System.Windows.Forms;
using UIDriver.CacheManagement;

namespace UIDriver.Visualization;

public sealed class TreeVisualizer : ITreeSnapshotSink
{
    public static TreeVisualizer Instance { get; } = new();

    private TreeVisualizerForm? _form;
    private readonly object _lock = new();

    private TreeVisualizer() { }

    public void OnSnapshot(ContainerId container, string title, TreeSnapshot snapshot)
    {
        EnsureStarted();
        _form!.RenderSnapshot(container, title, snapshot);
    }

    private void EnsureStarted()
    {
        lock (_lock)
        {
            if (_form != null) return;

            var ready = new ManualResetEventSlim();
            var thread = new Thread(() =>
            {
                _form = new TreeVisualizerForm();
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
