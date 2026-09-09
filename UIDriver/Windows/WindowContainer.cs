using CacheManagement;
using Interop.UIAutomationClient;
using UIDriver.CustomModels;

namespace UIDriver;

public sealed class WindowContainer : IDisposable
{
    public string WindowTitle { get; set; }
    public int[] WindowRunTimeId { get; set; }
    public int ProcessId { get; set; }

    private readonly WindowListener _windowListener;
    private readonly UICachedTreeManager _cachedTreeManager;

    public WindowContainer(IUIAutomationElement window, IUIAutomation automation)
    {
        try { WindowTitle = (string)window.GetCurrentPropertyValue((int)UiaProperty.Name); } catch { }
        WindowRunTimeId = window.LiveRuntimeId().Id;
        try { ProcessId = (int)window.GetCurrentPropertyValue((int)UiaProperty.ProcessId); } catch { }

        _windowListener = new WindowListener(window, automation);
        _cachedTreeManager = new UICachedTreeManager(automation);
        _cachedTreeManager.InitCachedTree(window);

        _windowListener.RegisterStructureChangedListener(_cachedTreeManager);
        _windowListener.RegisterPropertyChangedListener(_cachedTreeManager);
        _windowListener.StartListening();
    }

    public Task<IUIAutomationElement> SubmitOrderAsync(UIBy by) => throw new NotImplementedException();

    public UICachedTreeManager CacheTreeManager => _cachedTreeManager;

    public void RegisterToggleWindowEvent(ToggleWindowListener subscriber) => _windowListener.RegisterToggleWindowEvent(subscriber);

    public void Dispose() => _windowListener.Dispose();
}
