using FlaUI.Core.Identifiers;
using UIDriver.CustomModels;

namespace UIDriver;

public sealed class ToggleWindowListener
{
    private readonly UIApplicationManager _applicationManager;

    public ToggleWindowListener(UIApplicationManager applicationManager) => _applicationManager = applicationManager;

    public void NotifyOnOpened(UIAutomationElement window, EventId eventId)
    {
        LogEventFactory.RaiseWindowOpened(window.RunTimeId);
        _applicationManager.NotifyWindowOpened(window, eventId);
    }

    public void NotifyOnClosed(UIAutomationElement window, EventId eventId, RunTimeId windowRunTimeId)
    {
        LogEventFactory.RaiseWindowClosed(windowRunTimeId);
        _applicationManager.NotifyWindowClosed(windowRunTimeId, eventId);
    }
}
