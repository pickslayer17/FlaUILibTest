using Interop.UIAutomationClient;
using UIDriver.Constants;
using UIDriver.CustomModels;
using UIDriver.Visualization;

namespace UIDriver;

public sealed class WindowContainer : IDisposable
{
    public string WindowTitle { get; set; }
    public int[] WindowRunTimeId { get; set; }
    public int ProcessId { get; set; }

    public ContainerId Id { get; } = new();

    private readonly WindowListener _windowListener;
    private readonly UICachedTreeManager _cachedTreeManager;

    public WindowContainer(IUIAutomationElement window, IUIAutomation automation, ITreeSnapshotSink snapshotSink)
    {
        try { WindowTitle = (string)window.GetCurrentPropertyValue((int)UiaProperty.Name); } catch { }
        WindowRunTimeId = window.LiveRuntimeId().Id;
        try { ProcessId = (int)window.GetCurrentPropertyValue((int)UiaProperty.ProcessId); } catch { }

        _windowListener = new WindowListener(window, automation);
        _cachedTreeManager = new UICachedTreeManager(automation, Id, snapshotSink);
        _cachedTreeManager.InitCachedTree(window);

        _windowListener.RegisterStructureChangedListener(_cachedTreeManager);
        _windowListener.RegisterPropertyChangedListener(_cachedTreeManager);
        _windowListener.StartListening();
    }

    public Task<IUIAutomationElement> SubmitOrderAsync(UIBy by) => _cachedTreeManager.FindFirst(by);

    public void PublishInitialSnapshot() => _cachedTreeManager.PublishInitialSnapshot(WindowTitle);

    public UICachedTreeManager CacheTreeManager => _cachedTreeManager;

    public void RegisterToggleWindowEvent(ToggleWindowListener subscriber) => _windowListener.RegisterToggleWindowEvent(subscriber);

    public void Dispose() => _windowListener.Dispose();
}
