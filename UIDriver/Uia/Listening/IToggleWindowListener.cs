namespace UIDriver.Uia.Listening;

public interface IToggleWindowListener
{
    public void NotifyOnOpened(UiaElement window);

    public void NotifyOnClosed(RunTimeId windowRunTimeId);
}
