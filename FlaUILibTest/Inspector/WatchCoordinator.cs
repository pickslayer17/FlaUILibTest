using FlaUI.Core.AutomationElements;
using FlaUILibTest.Helpers;
using System.Collections.Concurrent;

namespace FlaUILibTest.Inspector;

// Единый «повелитель тасок». Один на всё приложение, окно-агностичный: держит все watch'и
// (без синхронного списка с локами — ConcurrentDictionary) и резолвит их по сигналу Poke от
// любого finder'а. Не знает ни про окна, ни про то, КАК искать — это всё внутри самих watch'ей.
public sealed class WatchCoordinator
{
    public int TimeOut = 15_000;

    private readonly ConcurrentDictionary<Guid, Watch> _watches = new();

    // Регистрирует отложенный поиск и ждёт его (с таймаутом контракта).
    // attempt — самодостаточная попытка: ищет от своего корня по своему условию.
    public async Task<AutomationElement> AwaitElementAsync(string label, Func<AutomationElement> attempt)
    {
        var watch = new Watch(label, attempt);
        _watches.TryAdd(watch.Id, watch);
        LogManager.Log($"{label} - RegisterAsync");

        try
        {
            if (!watch.TryResolve()) // немедленная попытка
                await watch.Task.WaitAsync(TimeSpan.FromMilliseconds(TimeOut));

            var result = await watch.Task;
            LogManager.Log($"{label} - RESOLVED");
            return result;
        }
        catch (TimeoutException)
        {
            LogManager.Log($"{label} - TIMEOUT after {TimeOut}ms");
            watch.Cancel();
            throw;
        }
        finally
        {
            _watches.TryRemove(watch.Id, out _);
        }
    }

    // Сигнал от finder'а: «в окне что-то изменилось». Перепроверяем все pending watch'и —
    // каждый ищет от своего корня, так что событие тут просто тактовый импульс.
    public void Poke()
    {
        foreach (var (id, watch) in _watches)
        {
            if (watch.TryResolve())
                _watches.TryRemove(id, out _);
        }
    }
}
