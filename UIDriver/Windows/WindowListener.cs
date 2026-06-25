using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.EventHandlers;
using FlaUI.Core.Identifiers;

namespace UIDriver;

public sealed class WindowListener : IDisposable
{
    private readonly AutomationElement _window;
    private readonly Watcher _watcher;

    private ToggleWindowListener? _toggleWindowSubscriber;

    private StructureChangedEventHandlerBase? _structure;
    private PropertyChangedEventHandlerBase? _property;
    private AutomationEventHandlerBase? _windowOpened;
    private AutomationEventHandlerBase? _windowClosed;

    public WindowListener(AutomationElementObject window, Watcher watcher)
    {
        _window = window.Element;
        _watcher = watcher;
    }

    public void RegisterOpenWindowEvent(ToggleWindowListener subscriber) => _toggleWindowSubscriber = subscriber;

    public void RegisterCloseWindowEvent(ToggleWindowListener subscriber) => _toggleWindowSubscriber = subscriber;

    public void StartListening()
    {
        _structure = _window.RegisterStructureChangedEvent(TreeScope.Subtree, OnStructureChanged);
        _property = _window.RegisterPropertyChangedEvent(TreeScope.Subtree, OnPropertyChanged, PropertiesToWatch());
        _windowOpened = _window.RegisterAutomationEvent(
            _window.Automation.EventLibrary.Window.WindowOpenedEvent, TreeScope.Subtree, OnWindowOpened);
        _windowClosed = _window.RegisterAutomationEvent(
            _window.Automation.EventLibrary.Window.WindowClosedEvent, TreeScope.Element, OnWindowClosed);
    }

    private void OnStructureChanged(AutomationElement element, StructureChangeType changeType, int[] runtimeId)
    {
        LogEventFactory.RaiseText($"Structure ");
        _watcher.PokeOnStructureChanged(new AutomationElementObject(element));
    }

    private void OnPropertyChanged(AutomationElement element, PropertyId propertyId, object newValue)
    {
        _watcher.PokeOnPropertyChanged(new AutomationElementObject(element));
    }

    private void OnWindowOpened(AutomationElement element, EventId eventId)
    {
        _toggleWindowSubscriber?.NotifyOnOpened(new AutomationElementObject(element));
    }

    private void OnWindowClosed(AutomationElement element, EventId eventId)
    {
        _toggleWindowSubscriber?.NotifyOnClosed(new AutomationElementObject(element));
    }

    private static PropertyId[] PropertiesToWatch()
        => UiaPropertyHelper.AllProperties
            .Except([UiaPropertyHelper.GetPropertyId(UiaProperty.BoundingRectangle)])
            .ToArray();

    public void Dispose()
    {
        // отписка от UIA-событий — добавится после сверки точного unregister-API FlaUI
    }
}
