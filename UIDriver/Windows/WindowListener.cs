using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.EventHandlers;
using FlaUI.Core.Identifiers;

namespace UIDriver;

public sealed class WindowListener : IDisposable
{
    private readonly AutomationElement _window;
    private readonly Watcher _watcher;
    private readonly IEventLibrary _eventLibrary;

    private ToggleWindowListener? _toggleWindowSubscriber;

    public WindowListener(AutomationElementObject window, Watcher watcher, IEventLibrary eventLibrary)
    {
        _window = window.Element;
        _watcher = watcher;
        _eventLibrary = eventLibrary;
    }

    public void RegisterOpenWindowEvent(ToggleWindowListener subscriber) => _toggleWindowSubscriber = subscriber;

    public void RegisterCloseWindowEvent(ToggleWindowListener subscriber) => _toggleWindowSubscriber = subscriber;

    public void StartListening()
    {
        _window.RegisterStructureChangedEvent(TreeScope.Subtree, OnStructureChanged);
        _window.RegisterPropertyChangedEvent(TreeScope.Subtree, OnPropertyChanged, PropertiesToWatch());
        _window.RegisterAutomationEvent(_eventLibrary.Window.WindowOpenedEvent, TreeScope.Subtree, OnWindowOpened);
        _window.RegisterAutomationEvent(_eventLibrary.Window.WindowClosedEvent, TreeScope.Element, OnWindowClosed);
    }

    private void OnStructureChanged(AutomationElement element, StructureChangeType changeType, int[] runtimeId)
    {
        _watcher.PokeOnStructureChanged(new AutomationElementObject(element));
    }

    private void OnPropertyChanged(AutomationElement element, PropertyId propertyId, object newValue)
    {
        _watcher.PokeOnPropertyChanged(new AutomationElementObject(element));
    }

    private void OnWindowOpened(AutomationElement element, EventId eventId)
    {
        _toggleWindowSubscriber?.NotifyOnOpened(new AutomationElementObject(element), eventId);
    }

    private void OnWindowClosed(AutomationElement element, EventId eventId)
    {
        _toggleWindowSubscriber?.NotifyOnClosed(new AutomationElementObject(element), eventId);
    }

    private static PropertyId[] PropertiesToWatch()
        => UiaPropertyHelper.AllProperties
            .Except([UiaPropertyHelper.GetPropertyId(UiaProperty.BoundingRectangle)])
            .ToArray();

    public void Dispose()
    {
    }
}
