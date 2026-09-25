using Microsoft.Extensions.Logging;
using UIDriver.Windows;

namespace UIDriver.Events.Handlers;

public sealed class WindowClosedHandler
{
    private readonly WindowRegistry _windowRegistry;
    private readonly ILogger<WindowClosedHandler> _logger;

    public WindowClosedHandler(WindowRegistry windowRegistry, ILogger<WindowClosedHandler> logger)
    {
        _windowRegistry = windowRegistry;
        _logger = logger;
    }

    public void Handle(WindowClosedEvent windowClosed)
    {
        if (!_windowRegistry.Contains(windowClosed.WindowRunTimeId))
            return;

        _logger.LogInformation("WINDOW CLOSED: [{Window}]", windowClosed.WindowRunTimeId);
        _windowRegistry.Remove(windowClosed.WindowRunTimeId);
    }
}
