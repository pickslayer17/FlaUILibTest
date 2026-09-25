using Interop.UIAutomationClient;
using UIDriver.Uia.Constants;

namespace UIDriver.Uia.Listening;

public sealed class WindowListener : IDisposable
{
    private readonly UiaAutomation _automation;
    private readonly UiaElement _window;
    private readonly RunTimeId _windowRunTimeId;
    private readonly List<IStructureChangedListener> _structureChangedListeners = new();
    private readonly List<IPropertyChangedListener> _propertyChangedListeners = new();

    private IToggleWindowListener? _toggleWindowListener;
    private readonly StructureChangeType[] _ignoredStructureChangeTypes =
    [
        StructureChangeType.StructureChangeType_ChildRemoved,
    ];
    private readonly UiaProperty[] _ignoredProperties =
    [
       UiaProperty.BoundingRectangle,
    ];

    private NativeStructureChangedHandler? _structureChangedHandler;
    private NativePropertyChangedHandler? _propertyChangedHandler;
    private NativeAutomationEventHandler? _windowOpenedHandler;
    private NativeAutomationEventHandler? _windowClosedHandler;

    public WindowListener(UiaElement window, RunTimeId windowRunTimeId, UiaAutomation automation)
    {
        _window = window;
        _windowRunTimeId = windowRunTimeId;
        _automation = automation;
    }

    public void RegisterStructureChangedListener(IStructureChangedListener structureChangedListener) => _structureChangedListeners.Add(structureChangedListener);

    public void RegisterPropertyChangedListener(IPropertyChangedListener propertyChangedListener) => _propertyChangedListeners.Add(propertyChangedListener);

    public void RegisterToggleWindowListener(IToggleWindowListener toggleWindowListener) => _toggleWindowListener = toggleWindowListener;

    public void StartListening()
    {
        var automation = _automation.Native;
        var window = _window.Native;

        _structureChangedHandler = new NativeStructureChangedHandler(OnStructureChanged);
        automation.AddStructureChangedEventHandler(window, TreeScope.TreeScope_Subtree, CacheProfile.Subtree.CreateRequest(_automation), _structureChangedHandler);

        _propertyChangedHandler = new NativePropertyChangedHandler(OnPropertyChanged);
        automation.AddPropertyChangedEventHandler(window, TreeScope.TreeScope_Subtree, CacheProfile.SingleElement.CreateRequest(_automation), _propertyChangedHandler, PropertiesToWatch());

        _windowOpenedHandler = new NativeAutomationEventHandler(OnWindowOpened);
        automation.AddAutomationEventHandler((int)UiaEvent.WindowOpened, window, TreeScope.TreeScope_Subtree, null, _windowOpenedHandler);

        _windowClosedHandler = new NativeAutomationEventHandler(OnWindowClosed);
        automation.AddAutomationEventHandler((int)UiaEvent.WindowClosed, window, TreeScope.TreeScope_Element, null, _windowClosedHandler);
    }

    private void OnStructureChanged(IUIAutomationElement element, StructureChangeType changeType, int[] runtimeId)
    {
        if (_ignoredStructureChangeTypes.Any(t => t == changeType))
            return;

        var source = _automation.Wrap(element);
        var targetRunTimeId = RunTimeId.FromArray(runtimeId);
        foreach (var structureChangedListener in _structureChangedListeners)
        {
            structureChangedListener.NotifyOnStructureChanged(source, (UiaStructureChangeType)changeType, targetRunTimeId);
        }
    }

    private void OnPropertyChanged(IUIAutomationElement element, int propertyId, object newValue)
    {
        if (_ignoredProperties.Any(p => (int)p == propertyId))
            return;

        var source = _automation.Wrap(element);
        foreach (var propertyChangedListener in _propertyChangedListeners)
        {
            propertyChangedListener.NotifyOnPropertyChanged(source, (UiaProperty)propertyId, newValue);
        }
    }

    private void OnWindowOpened(IUIAutomationElement element, int eventId)
    {
        _toggleWindowListener?.NotifyOnOpened(_automation.Wrap(element));
    }

    private void OnWindowClosed(IUIAutomationElement element, int eventId)
    {
        _toggleWindowListener?.NotifyOnClosed(_windowRunTimeId);
    }

    private static int[] PropertiesToWatch()
        => CacheProfile.SingleElement.Properties
            .Except([UiaProperty.RuntimeId])
            .Select(property => (int)property)
            .ToArray();

    public void Dispose()
    {
    }
}
