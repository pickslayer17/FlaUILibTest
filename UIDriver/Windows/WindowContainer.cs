using FlaUI.Core;

namespace UIDriver;

public sealed class WindowContainer : IDisposable
{
    public string WindowTitle { get; set; }
    public int[] WindowRunTimeId { get; set; }
    public int ProcessId { get; set; }

    private readonly Watcher _watcher;
    private readonly WindowManager _windowManager;
    private readonly WindowListener _windowListener;

    public WindowContainer(AutomationElementObject window, IEventLibrary eventLibrary)
    {
        try { WindowTitle = window.Element.Properties.Name; } catch { }
        WindowRunTimeId = window.RunTimeId.Id;
        ProcessId = window.Element.Properties.ProcessId; // id like to see exception here

        _watcher = new Watcher(window);
        _windowManager = new WindowManager(window, _watcher);
        _windowListener = new WindowListener(window, _watcher, eventLibrary);
        _windowListener.StartListening();
    }

    public Task<AutomationElementObject> SubmitOrderAsync(Order order) => _windowManager.ProcessOrderAsync(order);

    public void RegisterToggleWindowEvent(ToggleWindowListener subscriber) => _windowListener.RegisterToggleWindowEvent(subscriber);

    public void Dispose() => _windowListener.Dispose();
}
