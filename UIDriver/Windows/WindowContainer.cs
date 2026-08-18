using Interop.UIAutomationClient;
using UIDriver.Constants;

namespace UIDriver;

public sealed class WindowContainer : IDisposable
{
    public string WindowTitle { get; set; }
    public int[] WindowRunTimeId { get; set; }
    public int ProcessId { get; set; }

    private readonly WindowListener _windowListener;
    private readonly UICachedTreeManager _cachedTreeManager;

    public WindowContainer(UIAutomationElement window, IUIAutomation automation)
    {
        try { WindowTitle = (string)window.Element.GetCurrentPropertyValue((int)UiaProperty.Name); } catch { }
        WindowRunTimeId = window.RunTimeId.Id;
        try { ProcessId = (int)window.Element.GetCurrentPropertyValue((int)UiaProperty.ProcessId); } catch { }

        _windowListener = new WindowListener(window, automation);
        _cachedTreeManager = new UICachedTreeManager(automation);
        _cachedTreeManager.InitCachedTree(window.Element);

        _windowListener.RegisterStructureChangedListener(_cachedTreeManager);
        _windowListener.RegisterPropertyChangedListener(_cachedTreeManager);
        _windowListener.StartListening();
    }

    public Task<UIAutomationElement> SubmitOrderAsync(UIBy by) => _cachedTreeManager.FindFirst(by);

    public UICachedTreeManager CacheTreeManager => _cachedTreeManager;

    public void RegisterToggleWindowEvent(ToggleWindowListener subscriber) => _windowListener.RegisterToggleWindowEvent(subscriber);

    public void Dispose() => _windowListener.Dispose();
}
