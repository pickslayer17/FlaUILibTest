using UIDriver.Tree;
using UIDriver.Uia;
using UIDriver.Uia.Constants;
using UIDriver.Uia.Listening;

namespace UIDriver.Events;

public sealed class WindowEventForwarder : IStructureChangedListener, IPropertyChangedListener, IToggleWindowListener
{
    private readonly UICachedTree _cachedTree;
    private readonly EventQueue _eventQueue;

    public WindowEventForwarder(UICachedTree cachedTree, EventQueue eventQueue)
    {
        _cachedTree = cachedTree;
        _eventQueue = eventQueue;
    }

    public void NotifyOnStructureChanged(UiaElement source, UiaStructureChangeType changeType, RunTimeId? targetRunTimeId)
        => _eventQueue.Enqueue(new StructureChangedEvent(_cachedTree, source, changeType, targetRunTimeId));

    public void NotifyOnPropertyChanged(UiaElement source, UiaProperty property, object newValue)
        => _eventQueue.Enqueue(new PropertyChangedEvent(_cachedTree, source, property, newValue));

    public void NotifyOnOpened(UiaElement window)
        => _eventQueue.Enqueue(new WindowOpenedEvent(window));

    public void NotifyOnClosed(RunTimeId windowRunTimeId)
        => _eventQueue.Enqueue(new WindowClosedEvent(windowRunTimeId));
}
