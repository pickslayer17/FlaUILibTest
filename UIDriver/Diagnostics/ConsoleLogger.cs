namespace UIDriver;

public sealed class ConsoleLogger : ILogEventSubscriber
{
    public void On(LogEvent logEvent)
    {
        switch (logEvent)
        {
            case OrderCreated e: ToConsole($"[{e.OrderId:N}] order created: {e.By.Scope}"); break;
            case ElementResolved e: ToConsole($"[{e.OrderId:N}] element resolved"); break;
            case WindowOpened e: ToConsole($"window opened: {e.Window}"); break;
            case WindowClosed e: ToConsole($"window closed: {e.Window}"); break;
            case TextEvent e: ToConsole($"text: {e.text}"); break;
        }
    }

    private Lock _consoleLoc = new Lock();
    public void ToConsole(string log)
    {
        lock (_consoleLoc)
        {
            Console.WriteLine(log);
        }
    }
}
