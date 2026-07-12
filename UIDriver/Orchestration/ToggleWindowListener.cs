using UIDriver.CustomModels;

namespace UIDriver;

public sealed class ToggleWindowListener
{
    private readonly UIApplicationManager _applicationManager;

    public ToggleWindowListener(UIApplicationManager applicationManager) => _applicationManager = applicationManager;

    public void NotifyOnOpened(UIAutomationElement window)
    {
        _applicationManager.NotifyWindowOpened(window);
    }

    public void NotifyOnClosed(UIAutomationElement window, RunTimeId windowRunTimeId)
    {
        _applicationManager.NotifyWindowClosed(windowRunTimeId);
    }
}
