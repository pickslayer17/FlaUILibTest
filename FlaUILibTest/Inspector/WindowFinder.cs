using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core.EventHandlers;
using FlaUI.Core.Identifiers;
using FlaUILibTest.Extensions;
using FlaUILibTest.Helpers;
using FlaUILibTest.Interfaces;
using FlaUILibTest.UIDriver;

namespace FlaUILibTest.Inspector;

// Рабочая лошадка поиска для ОДНОГО окна: владеет UIA-подписками своего окна и умеет искать
// от заданного корня. Async-координацию отложенного поиска не держит — пинает единый
// WatchCoordinator, общий на все finder'ы.
public sealed class WindowFinder : IDisposable, IFinder
{
    public Action<WindowFinder, AutomationElement, EventId, int[]> OnWindowOpenedFunc;
    public Action<WindowFinder, AutomationElement, EventId, int[]> OnWindowClosedFunc;
    public Func<AutomationElement, ConditionBase, AutomationElement> SearchFunc { get; set; }

    private StructureChangedEventHandlerBase StructureChangedHandler { get; set; }
    private PropertyChangedEventHandlerBase PropertyChangedHandler { get; set; }
    private AutomationEventHandlerBase WindowOpenedEventHandler { get; set; }
    private AutomationEventHandlerBase WindowClosedEventHandler { get; set; }

    public AutomationElement Window { get; init; }
    public int[] RootRuntimeId { get; }
    public string Name { get; }

    private readonly Lock _searchLock = new();
    private readonly WatchCoordinator _watches;

    public WindowFinder(AutomationElement window, WatchCoordinator watches)
    {
        RootRuntimeId = window.Properties.RuntimeId.ValueOrDefault;
        Name = window.Properties.Name.ValueOrDefault;
        Window = window;
        _watches = watches;
    }

    public AutomationElement DefaultSearch(AutomationElement root, ConditionBase condition)
    {
        lock (_searchLock)
        {
            var found = SearchFunc?.Invoke(root, condition);
            return found;
        }
    }

    public void StartListening()
    {
        Subscribe();
    }

    // Откладываем поиск в единый координатор. Корень (это окно) и примитив поиска (DefaultSearch)
    // замыкаем прямо в попытку — координатору про них знать не надо.
    public Task<AutomationElement> RegisterAndGetElementAsync(BY condition)
        => _watches.AwaitElementAsync(
            $"{Name} [{condition.Condition}]",
            () => DefaultSearch(Window, condition.Condition));

    private void Subscribe()
    {
        StructureChangedHandler = Window.RegisterStructureChangedEvent(TreeScope.Subtree, OnStructureChanged);
        PropertyChangedHandler = Window.RegisterPropertyChangedEvent(TreeScope.Subtree, OnPropertyChanged, UiaPropertyHelper.AllProperties
            .Except(
                [
                    UiaPropertyHelper.GetPropertyId(UiaProperty.BoundingRectangle)
                ]
                ).ToArray());
        WindowOpenedEventHandler = Window.RegisterAutomationEvent(
            Window.Automation.EventLibrary.Window.WindowOpenedEvent,
            TreeScope.Subtree,
            OnWindowOpened);
        WindowClosedEventHandler = Window.RegisterAutomationEvent(
            Window.Automation.EventLibrary.Window.WindowClosedEvent,
            TreeScope.Element,
            OnWindowClosed);
    }

    private void OnStructureChanged(AutomationElement element, StructureChangeType changeType, int[] runtimeId)
    {
        if (!(changeType == StructureChangeType.ChildAdded ||
            changeType == StructureChangeType.ChildrenInvalidated)) return;

        _watches.Poke();
    }

    private void OnPropertyChanged(AutomationElement element, PropertyId propertyId, object newValue)
    {
        _watches.Poke();
    }

    private void OnWindowOpened(AutomationElement element, EventId eventId)
    {
        var info = GetElementInfo(element);
        Log($"WINDOW_OPENED | {info}");
        _watches.Poke();
        if (element.AsWindow().TryGetWindowRunTimeId(out int[] windowRunTimeId))
        {
            OnWindowOpenedFunc?.Invoke(this, element, eventId, windowRunTimeId);
        }
        else
        {
            LogManager.LogError("Failed to get RuntimeId");
        }
    }

    private void OnWindowClosed(AutomationElement element, EventId eventId)
    {
        var info = GetElementInfo(element);
        Log($"WINDOW_CLOSED | {info}");
        OnWindowClosedFunc?.Invoke(this, element, eventId, RootRuntimeId);
    }

    private string GetElementInfo(AutomationElement element)
    {
        var name = "";
        var cls = "";
        var ct = "";
        var aid = "";
        try { ct = element.Properties.ControlType.ValueOrDefault.ToString(); } catch { }
        try { aid = element.Properties.AutomationId.ValueOrDefault; } catch { }
        try { name = element.Properties.Name.ValueOrDefault; } catch { }
        try { cls = element.Properties.ClassName.ValueOrDefault; } catch { }
        return $"Nm: '{name}' | Cls: '{cls}' | CnrtrlTp: '{ct}' | AutId: '{aid}'";
    }

    private void Log(string message)
    {
        LogManager.Log($"{Name} - {message}");
    }

    public void Dispose()
    {
        Log("Disposing finder and cancelling pending watches[not implemented yet]");
    }
}
