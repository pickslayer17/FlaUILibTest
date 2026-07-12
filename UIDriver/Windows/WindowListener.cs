using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Identifiers;
using UIDriver.CustomModels;
using UIDriver.Interfaces;

namespace UIDriver;

public sealed class WindowListener : IDisposable
{
    private readonly AutomationElement _window;
    private readonly RunTimeId _windowRunTimeId;
    private List<IStructureChangedListener> _structureChangedListeners = new();
    private List<IPropertyChangedListener> _propertChangedListeners = new();

    private ToggleWindowListener? _toggleWindowSubscriber;
    private StructureChangeType[] _ignoredStructureChangeTypes =
    [
        StructureChangeType.ChildRemoved,
    ];
    private UiaProperty[] _ignoredProperties =
    [
       UiaProperty.BoundingRectangle,
    ];

    public WindowListener(UIAutomationElement window)
    {
        _window = window.Element;
        _windowRunTimeId = window.RunTimeId;
    }

    public void RegisterStructureChangedListener(IStructureChangedListener structureChangedListener) => _structureChangedListeners.Add(structureChangedListener);

    public void RegisterPropertyChangedListener(IPropertyChangedListener propertyChangedListener) => _propertChangedListeners.Add(propertyChangedListener);

    public void RegisterToggleWindowEvent(ToggleWindowListener subscriber) => _toggleWindowSubscriber = subscriber;

    public void StartListening()
    {
        _window.RegisterStructureChangedEvent(TreeScope.Subtree, OnStructureChanged);
        _window.RegisterPropertyChangedEvent(TreeScope.Subtree, OnPropertyChanged, PropertiesToWatch());
        _window.RegisterAutomationEvent(_window.Automation.EventLibrary.Window.WindowOpenedEvent, TreeScope.Subtree, OnWindowOpened);
        _window.RegisterAutomationEvent(_window.Automation.EventLibrary.Window.WindowClosedEvent, TreeScope.Element, OnWindowClosed);
    }

    private void OnStructureChanged(AutomationElement element, StructureChangeType changeType, int[] runtimeId)
    {
        if (_ignoredStructureChangeTypes.Any(t => t == changeType))
            return;

        foreach (var structureChangedListener in _structureChangedListeners)
        {
            structureChangedListener.NotifyOnStructureChanged(new UIAutomationElement(element), changeType, runtimeId);
        }
    }

    private void OnPropertyChanged(AutomationElement element, PropertyId propertyId, object newValue)
    {
        if (_ignoredProperties.Any(p => UiaPropertyHelper.GetPropertyId(p) == propertyId))
            return;

        foreach (var propertChangedListener in _propertChangedListeners)
        {
            propertChangedListener.NotifyOnPropertyChanged(new UIAutomationElement(element));
        }
    }

    private void OnWindowOpened(AutomationElement element, EventId eventId)
    {
        if (element.Properties.ProcessId.TryGetValue(out var processId) && processId != 0)
            LogEventFactory.RaiseText($"window opened with process id[{processId}]");

        _toggleWindowSubscriber?.NotifyOnOpened(new UIAutomationElement(element), eventId);
    }

    private void OnWindowClosed(AutomationElement element, EventId eventId)
    {
        _toggleWindowSubscriber?.NotifyOnClosed(new UIAutomationElement(element), eventId, _windowRunTimeId);
    }

    private static PropertyId[] PropertiesToWatch()
        => UiaPropertyHelper.AllProperties
            .Except([UiaPropertyHelper.GetPropertyId(UiaProperty.BoundingRectangle)])
            .ToArray();

    public void Dispose()
    {
    }
}
