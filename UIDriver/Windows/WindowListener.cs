using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Identifiers;

namespace UIDriver;

public sealed class WindowListener : IDisposable
{
    private readonly AutomationElement _window;
    private readonly RunTimeId _windowRunTimeId;
    private readonly Watcher _watcher;
    private readonly IEventLibrary _eventLibrary;

    private ToggleWindowListener? _toggleWindowSubscriber;
    private StructureChangeType[] _ignoredStructureChangeTypes =
    [
        StructureChangeType.ChildRemoved,
    ];
    private UiaProperty[] _ignoredProperties =
    [
       UiaProperty.BoundingRectangle,
    ];

    public WindowListener(AutomationElementObject window, Watcher watcher, IEventLibrary eventLibrary)
    {
        _window = window.Element;
        _windowRunTimeId = window.RunTimeId;
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
        if (_ignoredStructureChangeTypes.Any(t => t == changeType))
            return;

        _watcher.PokeOnStructureChanged(new AutomationElementObject(element));
    }

    private void OnPropertyChanged(AutomationElement element, PropertyId propertyId, object newValue)
    {
        if (_ignoredProperties.Any(p => UiaPropertyHelper.GetPropertyId(p) == propertyId))
            return;

        _watcher.PokeOnPropertyChanged(new AutomationElementObject(element));
    }

    private void OnWindowOpened(AutomationElement element, EventId eventId)
    {
        if (element.Properties.ProcessId.TryGetValue(out var processId) || processId != 0)
            LogEventFactory.RaiseText($"window opened with process id[{processId}]");

        _toggleWindowSubscriber?.NotifyOnOpened(new AutomationElementObject(element), eventId);
    }

    private void OnWindowClosed(AutomationElement element, EventId eventId)
    {
        _toggleWindowSubscriber?.NotifyOnClosed(new AutomationElementObject(element), eventId, _windowRunTimeId);
    }

    private static PropertyId[] PropertiesToWatch()
        => UiaPropertyHelper.AllProperties
            .Except([UiaPropertyHelper.GetPropertyId(UiaProperty.BoundingRectangle)])
            .ToArray();

    public void Dispose()
    {
    }
}
