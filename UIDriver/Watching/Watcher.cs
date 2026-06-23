using System.Collections.Concurrent;
using FlaUI.Core.AutomationElements;

namespace UIDriver;

// Один на окно. Держит все Watch'и своего окна (ConcurrentDictionary, без синхронного списка с локами)
// и резолвит их по сигналу Poke. Сам владеет async-ожиданием и таймаутами; finder'ы синхронные.
public sealed class Watcher
{
    private readonly ConcurrentDictionary<Guid, Watch> _watches = new();

    // Завести заявку и отдать её Task наверх (WindowManager → AppManager → Locator).
    public Task<AutomationElement> AddWatch(IFinder finder, AutomationElement source, TimeSpan timeout)
        => throw new NotImplementedException();

    // Сигнал от WindowListener: «в окне что-то изменилось» — перепроверить все pending Watch'и.
    public void Poke() => throw new NotImplementedException();
}
