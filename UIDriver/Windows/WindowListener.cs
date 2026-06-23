using FlaUI.Core.AutomationElements;

namespace UIDriver;

// Один на окно. Подписывается на UIA-события своего окна и разводит их:
// Struct/Prop Changed → Watcher.Poke; Window open/close → наверх в AppManager (через колбэки).
public sealed class WindowListener : IDisposable
{
    private readonly AutomationElement _window;
    private readonly Watcher _watcher;

    public WindowListener(AutomationElement window, Watcher watcher)
    {
        _window = window;
        _watcher = watcher;
    }

    // Открытие/закрытие окон уходит наверх — AppManager создаёт/удаляет контейнеры.
    public Action<AutomationElement>? OnWindowOpened { get; set; }
    public Action<AutomationElement>? OnWindowClosed { get; set; }

    public void Subscribe() => throw new NotImplementedException();

    public void Dispose() => throw new NotImplementedException();
}
