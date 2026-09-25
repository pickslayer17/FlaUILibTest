using Interop.UIAutomationClient;
using UIDriver.CustomModels;

namespace UIDriver;

public sealed class ToggleWindowListener
{
    private readonly UIApplicationManager _applicationManager;

    public ToggleWindowListener(UIApplicationManager applicationManager) => _applicationManager = applicationManager;

    public void NotifyOnOpened(IUIAutomationElement window)
    {
        _applicationManager.NotifyWindowOpened(window);
    }

    public void NotifyOnClosed(IUIAutomationElement window, RunTimeId windowRunTimeId)
    {
        _applicationManager.NotifyWindowClosed(windowRunTimeId);
    }
}
