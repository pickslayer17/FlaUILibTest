using System.Collections.Concurrent;
using FlaUI.Core.AutomationElements;

namespace UIDriver;

// Верхний уровень. Пока ТОЛЬКО роутинг BY по контейнерам: знает, какой RuntimeId окна у какого
// контейнера, кто Default, кто Desktop. Живых окон наружу не отдаёт — только передаёт им BY.
// Держит свои Orders (что просит Locator).
public sealed class ApplicationManager
{
    private readonly ConcurrentDictionary<string, WindowContainer> _containers = new(); // ключ: RuntimeId окна
    private readonly ConcurrentDictionary<Guid, Order> _orders = new();

    private WindowContainer? _default;
    private WindowContainer? _desktop;

    // Locator сдаёт сюда BY; маршрутизируем в нужный контейнер по BY.Scope.
    public Task<AutomationElement> Submit(BY by) => throw new NotImplementedException();
}
