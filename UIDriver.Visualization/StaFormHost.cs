namespace UIDriver.Visualization;

internal sealed class StaFormHost<TForm> where TForm : Form, new()
{
    private readonly object _lock = new();
    private TForm? _form;

    public TForm StartedForm
    {
        get
        {
            EnsureStarted();
            return _form!;
        }
    }

    private void EnsureStarted()
    {
        lock (_lock)
        {
            if (_form != null) return;

            var ready = new ManualResetEventSlim();
            var thread = new Thread(() =>
            {
                _form = new TForm();
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
