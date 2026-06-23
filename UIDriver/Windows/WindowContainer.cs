namespace UIDriver;

public sealed class WindowContainer : IDisposable
{
    private readonly Watcher _watcher;
    private readonly WindowManager _windowManager;
    private readonly WindowListener _windowListener;

    public WindowContainer(AutomationElementObject window)
    {
        _watcher = new Watcher();
        _windowManager = new WindowManager(window, _watcher);
        _windowListener = new WindowListener(window, _watcher);
        _windowListener.StartListening();
    }

    public Task<AutomationElementObject> Accept(Order order) => _windowManager.Accept(order);

    public void RegisterOpenWindowEvent(ToggleWindowListener subscriber) => _windowListener.RegisterOpenWindowEvent(subscriber);

    public void RegisterCloseWindowEvent(ToggleWindowListener subscriber) => _windowListener.RegisterCloseWindowEvent(subscriber);

    public void Dispose() => _windowListener.Dispose();
}
