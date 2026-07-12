using System.Windows.Forms;

namespace UIDriver.Visualization;

public static class TreeVisualizer
{
    private static TreeVisualizerForm? _form;
    private static readonly object _lock = new();

    public static void EnsureStarted()
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

    public static void Render(IReadOnlyList<(string Title, UiNode Tree)> containers)
    {
        EnsureStarted();
        _form!.Render(containers);
    }
}
