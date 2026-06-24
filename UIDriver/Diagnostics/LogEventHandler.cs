namespace UIDriver;

public static class LogEventHandler
{
    private static readonly Lock _lock = new();
    private static readonly List<ILogEventSubscriber> _subscribers = new();

    public static void Subscribe(ILogEventSubscriber subscriber)
    {
        lock (_lock)
            _subscribers.Add(subscriber);
    }

    public static void Handle(LogEvent logEvent)
    {
        lock (_lock)
        {
            try
            {
                foreach (var subscriber in _subscribers)
                    subscriber.On(logEvent);
            }
            catch
            {
            }
        }
    }
}
