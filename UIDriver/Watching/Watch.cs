using FlaUI.Core.AutomationElements;

namespace UIDriver;

public enum WatchStatus
{
    Pending,
    Completed,
    TimedOut,
    Cancelled
}

// Одна заявка на поиск одной сущности. Несёт готовый IFinder и source («откуда искать»),
// свой таймаут и результат. Watcher на каждый Poke зовёт Finder.Find(Source) — один синхронный проход.
public sealed class Watch
{
    public IFinder Finder { get; }
    public AutomationElement Source { get; }
    public TimeSpan Timeout { get; }
    public WatchStatus Status { get; private set; } = WatchStatus.Pending;

    public Watch(IFinder finder, AutomationElement source, TimeSpan timeout)
    {
        Finder = finder;
        Source = source;
        Timeout = timeout;
    }

    // Awaitable результат заявки — его в итоге ждёт Locator.
    public Task<AutomationElement> Task => throw new NotImplementedException();

    // Один проход: дёрнуть Finder.Find(Source); нашёл → Completed. Без ожидания.
    public bool TryResolve() => throw new NotImplementedException();
}
