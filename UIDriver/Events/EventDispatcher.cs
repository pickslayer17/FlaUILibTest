using UIDriver.Events.Handlers;
using UIDriver.Uia.Constants;

namespace UIDriver.Events;

public sealed class EventDispatcher
{
    private readonly ChildAddedHandler _childAddedHandler;
    private readonly ChildrenInvalidatedHandler _childrenInvalidatedHandler;
    private readonly PropertyChangedHandler _propertyChangedHandler;
    private readonly WindowOpenedHandler _windowOpenedHandler;
    private readonly WindowClosedHandler _windowClosedHandler;

    public EventDispatcher(
        ChildAddedHandler childAddedHandler,
        ChildrenInvalidatedHandler childrenInvalidatedHandler,
        PropertyChangedHandler propertyChangedHandler,
        WindowOpenedHandler windowOpenedHandler,
        WindowClosedHandler windowClosedHandler)
    {
        _childAddedHandler = childAddedHandler;
        _childrenInvalidatedHandler = childrenInvalidatedHandler;
        _propertyChangedHandler = propertyChangedHandler;
        _windowOpenedHandler = windowOpenedHandler;
        _windowClosedHandler = windowClosedHandler;
    }

    public void Dispatch(DriverEvent driverEvent)
    {
        switch (driverEvent)
        {
            case StructureChangedEvent structureChanged:
                DispatchStructureChanged(structureChanged);
                break;
            case PropertyChangedEvent propertyChanged:
                _propertyChangedHandler.Handle(propertyChanged);
                break;
            case WindowOpenedEvent windowOpened:
                _windowOpenedHandler.Handle(windowOpened);
                break;
            case WindowClosedEvent windowClosed:
                _windowClosedHandler.Handle(windowClosed);
                break;
            default:
                throw new NotImplementedException($"No handler for {driverEvent.GetType().Name}.");
        }
    }

    private void DispatchStructureChanged(StructureChangedEvent structureChanged)
    {
        switch (structureChanged.ChangeType)
        {
            case UiaStructureChangeType.ChildAdded:
                _childAddedHandler.Handle(structureChanged);
                break;
            case UiaStructureChangeType.ChildrenInvalidated:
                _childrenInvalidatedHandler.Handle(structureChanged);
                break;
            default:
                throw new NotImplementedException($"No handler for structure change {structureChanged.ChangeType}.");
        }
    }
}
