namespace UIDriver;

public sealed class ConsoleLogger : ILogEventSubscriber
{
    public void On(LogEvent logEvent)
    {
        switch (logEvent)
        {
            case OrderCreated e: Console.WriteLine($"[{e.OrderId:N}] order created: {e.By.Scope}"); break;
            case ElementResolved e: Console.WriteLine($"[{e.OrderId:N}] element resolved"); break;
            case WindowOpened e: Console.WriteLine($"window opened: {e.Window}"); break;
            case WindowClosed e: Console.WriteLine($"window closed: {e.Window}"); break;
            case WindowEventBase e: Console.WriteLine($"window evnt: {e.Window}"); break;
            case TextEvent e: Console.WriteLine($"text: {e.text}"); break;
        }
    }
}
