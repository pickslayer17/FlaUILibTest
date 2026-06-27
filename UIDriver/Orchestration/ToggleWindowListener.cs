using FlaUI.Core.Identifiers;

namespace UIDriver;

public sealed class ToggleWindowListener
{
    private readonly ApplicationManager _applicationManager;

    public ToggleWindowListener(ApplicationManager applicationManager) => _applicationManager = applicationManager;

    public void NotifyOnOpened(AutomationElementObject window, EventId eventId)
    {
        LogEventFactory.RaiseWindowOpened(window.RunTimeId);
        _applicationManager.NotifyWindowOpened(window, eventId);
    }

    public void NotifyOnClosed(AutomationElementObject window, EventId eventId)
    {
        LogEventFactory.RaiseWindowClosed(window.RunTimeId);
        _applicationManager.NotifyWindowClosed(window.RunTimeId, eventId);
    }
}
