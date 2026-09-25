using Microsoft.Extensions.Logging;
using UIDriver.Exceptions;
using UIDriver.Windows;

namespace UIDriver.Events.Handlers;

public sealed class WindowOpenedHandler
{
    private readonly WindowRegistry _windowRegistry;
    private readonly WindowContainerFactory _windowContainerFactory;
    private readonly ILogger<WindowOpenedHandler> _logger;

    public WindowOpenedHandler(WindowRegistry windowRegistry, WindowContainerFactory windowContainerFactory, ILogger<WindowOpenedHandler> logger)
    {
        _windowRegistry = windowRegistry;
        _windowContainerFactory = windowContainerFactory;
        _logger = logger;
    }

    public void Handle(WindowOpenedEvent windowOpened)
    {
        var window = windowOpened.Window;
        var windowRunTimeId = window.Live.RunTimeId
            ?? throw new WindowRegistryException("Invalid window RuntimeId");

        if (_windowRegistry.Contains(windowRunTimeId))
            return;

        _logger.LogInformation("WINDOW OPENED: [{Window}]", windowRunTimeId);
        _windowRegistry.Add(_windowContainerFactory.Create(window));
    }
}
