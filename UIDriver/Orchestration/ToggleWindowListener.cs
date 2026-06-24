namespace UIDriver;

public sealed class ToggleWindowListener
{
    private readonly ApplicationManager _applicationManager;

    public ToggleWindowListener(ApplicationManager applicationManager) => _applicationManager = applicationManager;

    public void NotifyOnOpened(AutomationElementObject window)
    {
        LogEventFactory.RaiseWindowOpened(window.RunTimeId);
        _applicationManager.CreateWindowContainer(window);
    }

    public void NotifyOnClosed(AutomationElementObject window)
    {
        LogEventFactory.RaiseWindowClosed(window.RunTimeId);
        _applicationManager.RemoveWindowContainer(window.RunTimeId);
    }
}
