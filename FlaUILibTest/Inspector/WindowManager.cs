using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Identifiers;
using FlaUI.UIA3;
using FlaUILibTest.Constants;
using FlaUILibTest.Extensions;
using FlaUILibTest.Helpers;
using FlaUILibTest.Interfaces;
using FlaUILibTest.UIDriver;
using System.Collections.Concurrent;

namespace FlaUILibTest.Inspector;

public class WindowManager
{
    private readonly Lock _desktopLock = new();
    private readonly Lock _rootWindowLock = new ();
    private readonly Lock _searchLock = new();
    private readonly Lock _windowEventLock = new();
    private readonly Lock _finderCreateLock = new();

    private int[] DesktopRuntimeId
    {
        get
        {
            if (field == null || field.ToFormattedString() == "")
                throw new Exception("Desktop RuntimeId has not been set yet.");

            return field;
        }
        set
        {
            lock(_desktopLock)
            {
                if (!(field == null || field.ToFormattedString() == "")) throw new Exception("trying set desktop runtimeid again");
                field = value;
            }
        }
    }
    private int[] RootWindowRuntimeId
    {
        get
        {
            if (field == null || field.ToFormattedString() == "")
                throw new Exception("Root window RuntimeId has not been set yet.");

            return field;
        }
        set
        {
            lock (_rootWindowLock)
            {
                if (!(field == null || field.ToFormattedString() == "")) throw new Exception("trying set desktop runtimeid again");
                field = value;
            }
        }
    }
    private ConcurrentDictionary<WindowRunTimeId, WindowFinder> WindowFinders {
        get
        {
            return field;
        }
    } = new();

    public WindowManager()
    {
    }

    public IElementSource CreateSource(BY windowBy = null) => new WindowElementSource(this, windowBy);

    public FinderBase GetRootWindowFinder() => GetFinderByWindowId(RootWindowRuntimeId);

    public async Task<FinderBase> FindInDesktop(BY windowBy)
    {
        var desktopFinder = GetFinderByWindowId(DesktopRuntimeId);
        var window = await desktopFinder.RegisterAndGetElementAsync(windowBy);

        if (!window.TryGetWindowRunTimeId(out int[] windowRunTimeId))
            throw new Exception($"Failed to get RuntimeId for window found with condition [{windowBy}]. Cannot create finder for this window.");

        var key = windowRunTimeId.ToWindowRunTimeId();

        lock (_finderCreateLock)
        {
            if (WindowFinders.TryGetValue(key, out var existing))
                return existing;

            CreateWindowFinder(window);
            WindowFinders.TryGetValue(key, out var created);
            return created;
        }
    }

    private WindowFinder GetFinderByWindowId(int[] windowRunTimeId)
    {
        if (!WindowFinders.TryGetValue(windowRunTimeId.ToWindowRunTimeId(), out var finder))
        {
            LogManager.LogError("Desktop finder not found. Unable to perform search.");
            throw new Exception("Desktop finder not found. Unable to perform search.");
        }

        return finder;
    }

    public void  CreateWindowFinder(AutomationElement window, FinderTypes finderType = FinderTypes.Window)
    {
        var finder = new WindowFinder(window)
        {
            OnWindowOpenedFunc = WindowOpened,
            OnWindowClosedFunc = WindowClosed,
            SearchFunc = FindFirst
        };

        if(!window.TryGetWindowRunTimeId(out var windowRunTimeId)) 
            throw new Exception($"Failed to get RuntimeId for window [{finder.Name}]. Finder creation aborted.");

        AddWindowFinder(windowRunTimeId, finder);
        switch(finderType)
        {
            case FinderTypes.Desktop:
                DesktopRuntimeId = windowRunTimeId;
                break;
            case FinderTypes.RootWindow:
                RootWindowRuntimeId = windowRunTimeId;
                break;
            case FinderTypes.Window:
                break;
            case FinderTypes.Element:
                break;
             default:
                throw new Exception($"Unsupported finder type [{finderType}]");
        }
        finder.StartListening();

        LogManager.Log($"Finder created for window [{finder.Name}] with RuntimeId [{string.Join(",", finder.RootRuntimeId)}]");
    }
    
    private void AddWindowFinder(int[] windowRunTimeId, WindowFinder finder)
    {
        // some checks here to ensure we don't add duplicates or invalid finders could be added here in the future if needed

        WindowFinders.TryAdd(windowRunTimeId.ToWindowRunTimeId(), finder);
    }

    private void WindowOpened(FinderBase finder, AutomationElement eventElement, EventId eventId, int[] windowRunTimeId)
    {
        lock (_windowEventLock)
        {
            LogManager.Log("window opened");
        }
    }

    private void WindowClosed(FinderBase finder, AutomationElement eventElement, EventId eventId, int[] windowRunTimeId)
    {
        lock (_windowEventLock)
        {
            LogManager.Log("window closed");
        }
    }

    private AutomationElement FindFirst(AutomationElement root, ConditionBase condition)
    {
        lock (_searchLock)
        {
            return root.FindFirstDescendant(condition);
        }
    }

    private AutomationElement[] FindAll(AutomationElement root, ConditionBase condition)
    {
        lock (_searchLock)
        {
            return root.FindAllDescendants(condition);
        }
    }
}

public record WindowRunTimeId
{
    private readonly int[] _runtimeId;
    public WindowRunTimeId(int[] runtimeId)
    {
        _runtimeId = runtimeId;
    }

    public int[] RuntimeId => new List<int>(_runtimeId).ToArray();

    public override string ToString()
    {
        return _runtimeId == null? null : $"[{string.Join(",", _runtimeId)}]";
    }

    public static implicit operator string(WindowRunTimeId windowRunTimeId) => windowRunTimeId.ToString();
}