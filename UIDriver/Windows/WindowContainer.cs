using Interop.UIAutomationClient;
using UIDriver.Constants;

namespace UIDriver;

public sealed class WindowContainer : IDisposable
{
    public string WindowTitle { get; set; }
    public int[] WindowRunTimeId { get; set; }
    public int ProcessId { get; set; }

    private readonly UIWatcher _watcher;
    private readonly OrderProcesser _orderProcesser;
    private readonly WindowListener _windowListener;
    private readonly UICachedTreeManager _cachedTreeManager;

    public WindowContainer(UIAutomationElement window)
    {
        try { WindowTitle = (string)window.Element.GetCurrentPropertyValue((int)UiaProperty.Name); } catch { }
        WindowRunTimeId = window.RunTimeId.Id;
        try { ProcessId = (int)window.Element.GetCurrentPropertyValue((int)UiaProperty.ProcessId); } catch { }

        _watcher = new UIWatcher(window);
        _orderProcesser = new OrderProcesser(_watcher);
        _windowListener = new WindowListener(window);
        _cachedTreeManager = new UICachedTreeManager(UIAutomationProvider.Automation);
        _windowListener.RegisterStructureChangedListener(_watcher);
        _windowListener.RegisterPropertyChangedListener(_watcher);
        _windowListener.RegisterStructureChangedListener(_cachedTreeManager);
        _windowListener.StartListening();
    }

    public Task<UIAutomationElement> SubmitOrderAsync(Order order) => _orderProcesser.ProcessOrderAsync(order);

    public void RegisterToggleWindowEvent(ToggleWindowListener subscriber) => _windowListener.RegisterToggleWindowEvent(subscriber);

    public void Dispose() => _windowListener.Dispose();
}
