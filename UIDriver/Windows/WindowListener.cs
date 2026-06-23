using FlaUI.Core.AutomationElements;

namespace UIDriver;

public sealed class WindowListener : IDisposable
{
    private readonly AutomationElement _window;
    private readonly Watcher _watcher;

    public WindowListener(AutomationElement window, Watcher watcher)
    {
        _window = window;
        _watcher = watcher;
    }

    public Action<AutomationElement>? OnWindowOpened { get; set; }
    public Action<AutomationElement>? OnWindowClosed { get; set; }

    public void Subscribe() => throw new NotImplementedException();

    public void Dispose() => throw new NotImplementedException();
}
