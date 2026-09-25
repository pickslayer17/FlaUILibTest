using Microsoft.Extensions.Logging;
using UIDriver.Api;
using UIDriver.Diagnostics;
using UIDriver.Events;
using UIDriver.Events.Handlers;
using UIDriver.Uia;

namespace UIDriver.Windows;

public sealed class UIApplicationManager : IDisposable
{
    private readonly UiaAutomation _automation;
    private readonly WindowRegistry _windowRegistry = new();
    private readonly EventQueue _eventQueue = new();
    private readonly WindowContainerFactory _windowContainerFactory;

    public UIApplicationManager(UiaAutomation automation, SnapshotPublisher snapshotPublisher, ILoggerFactory loggerFactory)
    {
        _automation = automation;
        _windowContainerFactory = new WindowContainerFactory(automation, _eventQueue, snapshotPublisher);
        _eventQueue.Start(CreateEventDispatcher(snapshotPublisher, loggerFactory));
    }

    public int ProcessId
    {
        get => _windowRegistry.ProcessId;
        set => _windowRegistry.ProcessId = value;
    }

    public void RegisterDefault(UiaElement window) => _windowRegistry.RegisterDefault(_windowContainerFactory.Create(window));

    public void RegisterDesktop(UiaElement window) => _windowRegistry.RegisterDesktop(_windowContainerFactory.Create(window));

    public Task<UiaElement> RequestElementAsync(UIBy by) => _windowRegistry.DefaultContainer!.SubmitOrderAsync(by);

    public void Dispose()
    {
        _automation.RemoveAllEventHandlers();
        _eventQueue.Dispose();
        _windowRegistry.Dispose();
    }

    private EventDispatcher CreateEventDispatcher(SnapshotPublisher snapshotPublisher, ILoggerFactory loggerFactory) => new(
        new ChildAddedHandler(snapshotPublisher, loggerFactory.CreateLogger<ChildAddedHandler>()),
        new ChildrenInvalidatedHandler(snapshotPublisher, loggerFactory.CreateLogger<ChildrenInvalidatedHandler>()),
        new PropertyChangedHandler(),
        new WindowOpenedHandler(_windowRegistry, _windowContainerFactory, loggerFactory.CreateLogger<WindowOpenedHandler>()),
        new WindowClosedHandler(_windowRegistry, loggerFactory.CreateLogger<WindowClosedHandler>()));
}
