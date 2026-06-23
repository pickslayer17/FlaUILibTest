using FlaUI.Core.AutomationElements;

namespace UIDriver;

public sealed class WindowContainer
{
    public required AutomationElement Window { get; init; }
    public required WindowManager Manager { get; init; }
    public required WindowListener Listener { get; init; }
    public required Watcher Watcher { get; init; }
}
