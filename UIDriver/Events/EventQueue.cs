using System.Collections.Concurrent;

namespace UIDriver.Events;

public sealed class EventQueue : IDisposable
{
    private readonly BlockingCollection<DriverEvent> _events = new();
    private Thread? _consumerThread;

    public void Start(EventDispatcher dispatcher)
    {
        if (_consumerThread != null)
            throw new InvalidOperationException("Event queue is already started.");

        _consumerThread = new Thread(() => Consume(dispatcher))
        {
            IsBackground = true,
            Name = "UIDriver event queue"
        };
        _consumerThread.Start();
    }

    public void Enqueue(DriverEvent driverEvent) => _events.Add(driverEvent);

    private void Consume(EventDispatcher dispatcher)
    {
        foreach (var driverEvent in _events.GetConsumingEnumerable())
            dispatcher.Dispatch(driverEvent);
    }

    public void Dispose()
    {
        _events.CompleteAdding();
        _consumerThread?.Join();
        _events.Dispose();
    }
}
