namespace UIDriver;

public interface ILogEventSubscriber
{
    void On(LogEvent logEvent);
}
