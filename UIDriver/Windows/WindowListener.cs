using Interop.UIAutomationClient;
using UIDriver.Constants;
using UIDriver.CustomModels;
using UIDriver.Interfaces;

namespace UIDriver;

public sealed class WindowListener : IDisposable
{
    private readonly IUIAutomation _automation;
    private readonly IUIAutomationElement _window;
    private readonly RunTimeId _windowRunTimeId;
    private List<IStructureChangedListener> _structureChangedListeners = new();
    private List<IPropertyChangedListener> _propertChangedListeners = new();

    private ToggleWindowListener? _toggleWindowSubscriber;
    private StructureChangeType[] _ignoredStructureChangeTypes =
    [
        StructureChangeType.StructureChangeType_ChildRemoved,
    ];
    private UiaProperty[] _ignoredProperties =
    [
       UiaProperty.BoundingRectangle,
    ];

    private NativeStructureChangedHandler? _structureChangedHandler;
    private NativePropertyChangedHandler? _propertyChangedHandler;
    private NativeAutomationEventHandler? _windowOpenedHandler;
    private NativeAutomationEventHandler? _windowClosedHandler;

    public WindowListener(UIAutomationElement window, IUIAutomation automation)
    {
        _window = window.Element;
        _windowRunTimeId = window.RunTimeId;
        _automation = automation;
    }

    public void RegisterStructureChangedListener(IStructureChangedListener structureChangedListener) => _structureChangedListeners.Add(structureChangedListener);

    public void RegisterPropertyChangedListener(IPropertyChangedListener propertyChangedListener) => _propertChangedListeners.Add(propertyChangedListener);

    public void RegisterToggleWindowEvent(ToggleWindowListener subscriber) => _toggleWindowSubscriber = subscriber;

    public void StartListening()
    {
        var structureCacheRequest = _automation.CreateCacheRequest();
        structureCacheRequest.TreeScope = TreeScope.TreeScope_Subtree;
        structureCacheRequest.AutomationElementMode = AutomationElementMode.AutomationElementMode_Full;
        structureCacheRequest.AddProperty((int)UiaProperty.RuntimeId);
        structureCacheRequest.AddProperty((int)UiaProperty.ControlType);
        structureCacheRequest.AddProperty((int)UiaProperty.Name);

        _structureChangedHandler = new NativeStructureChangedHandler(OnStructureChanged);
        _automation.AddStructureChangedEventHandler(_window, TreeScope.TreeScope_Subtree, structureCacheRequest, _structureChangedHandler);

        _propertyChangedHandler = new NativePropertyChangedHandler(OnPropertyChanged);
        _automation.AddPropertyChangedEventHandler(_window, TreeScope.TreeScope_Subtree, null, _propertyChangedHandler, PropertiesToWatch());

        _windowOpenedHandler = new NativeAutomationEventHandler(OnWindowOpened);
        _automation.AddAutomationEventHandler((int)UiaEvent.WindowOpened, _window, TreeScope.TreeScope_Subtree, null, _windowOpenedHandler);

        _windowClosedHandler = new NativeAutomationEventHandler(OnWindowClosed);
        _automation.AddAutomationEventHandler((int)UiaEvent.WindowClosed, _window, TreeScope.TreeScope_Element, null, _windowClosedHandler);
    }

    private void OnStructureChanged(IUIAutomationElement element, StructureChangeType changeType, int[] runtimeId)
    {
        if (_ignoredStructureChangeTypes.Any(t => t == changeType))
            return;

        foreach (var structureChangedListener in _structureChangedListeners)
        {
            structureChangedListener.NotifyOnStructureChanged(new UIAutomationElement(element), changeType, runtimeId);
        }
    }

    private void OnPropertyChanged(IUIAutomationElement element, int propertyId, object newValue)
    {
        if (_ignoredProperties.Any(p => (int)p == propertyId))
            return;

        foreach (var propertChangedListener in _propertChangedListeners)
        {
            propertChangedListener.NotifyOnPropertyChanged(new UIAutomationElement(element));
        }
    }

    private void OnWindowOpened(IUIAutomationElement element, int eventId)
    {
        _toggleWindowSubscriber?.NotifyOnOpened(new UIAutomationElement(element));
    }

    private void OnWindowClosed(IUIAutomationElement element, int eventId)
    {
        _toggleWindowSubscriber?.NotifyOnClosed(new UIAutomationElement(element), _windowRunTimeId);
    }

    private static int[] PropertiesToWatch()
        => UiaPropertyHelper.AllProperties
            .Except([(int)UiaProperty.BoundingRectangle])
            .ToArray();

    public void Dispose()
    {
    }
}
