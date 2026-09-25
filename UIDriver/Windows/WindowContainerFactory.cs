using UIDriver.Diagnostics;
using UIDriver.Events;
using UIDriver.Uia;

namespace UIDriver.Windows;

public sealed class WindowContainerFactory
{
    private readonly UiaAutomation _automation;
    private readonly EventQueue _eventQueue;
    private readonly SnapshotPublisher _snapshotPublisher;

    public WindowContainerFactory(UiaAutomation automation, EventQueue eventQueue, SnapshotPublisher snapshotPublisher)
    {
        _automation = automation;
        _eventQueue = eventQueue;
        _snapshotPublisher = snapshotPublisher;
    }

    public WindowContainer Create(UiaElement window) => new(window, _automation, _eventQueue, _snapshotPublisher);
}
