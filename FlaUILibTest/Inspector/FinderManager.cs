using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Identifiers;
using FlaUI.UIA3;
using FlaUILibTest.Constants;
using FlaUILibTest.Extensions;
using FlaUILibTest.Helpers;
using FlaUILibTest.Interfaces;
using FlaUILibTest.UIDriver;
using FlaUIMonitor;
using System.Collections.Concurrent;

namespace FlaUILibTest.Inspector;

public class FinderManager
{
    private readonly Lock _desktopLock = new();
    private readonly Lock _rootWindowLock = new ();
    private readonly Lock _searchLock = new();
    private readonly Lock _finderCreateLock = new();

    // Единый координатор отложенного поиска — один на все finder'ы.
    private readonly WatchCoordinator _coordinator = new();

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

    public FinderManager()
    {
    }

    public IElementSource CreateSource(BY windowBy = null) => new WindowElementSource(this, windowBy);

    // Иммутабельный слепок текущего реестра finder'ов для монитора (Name + RuntimeId).
    // Без _finderCreateLock: ConcurrentDictionary.Values сам отдаёт потокобезопасный снимок;
    // массив клонируем, чтобы DTO не делил состояние с живым finder'ом.
    public IReadOnlyList<FinderSnapshot> SnapshotFinders()
        => WindowFinders.Values
            .Select(f => new FinderSnapshot(f.Name, (int[])(f.RootRuntimeId?.Clone() ?? Array.Empty<int>())))
            .ToList();

    public IFinder GetRootWindowFinder() => GetFinderByWindowId(RootWindowRuntimeId);

    public async Task<IFinder> FindInDesktop(BY windowBy)
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

    public void CreateWindowFinder(AutomationElement window, FinderTypes finderType = FinderTypes.Window)
    {
        var finder = new WindowFinder(window, _coordinator)
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
        MonitorHelper.Publish(SnapshotFinders());
    }
    
    private void AddWindowFinder(int[] windowRunTimeId, WindowFinder finder)
    {
        // some checks here to ensure we don't add duplicates or invalid finders could be added here in the future if needed

        WindowFinders.TryAdd(windowRunTimeId.ToWindowRunTimeId(), finder);
    }

    private void WindowOpened(WindowFinder finder, AutomationElement eventElement, EventId eventId, int[] windowRunTimeId)
    {
        lock (_finderCreateLock)
        {
            var key = windowRunTimeId.ToWindowRunTimeId();
            if (WindowFinders.ContainsKey(key))
            {
                LogManager.Log($"window opened, finder already exists [{key}]");
                return;
            }

            CreateWindowFinder(eventElement);
            LogManager.Log($"window opened, finder created [{key}]");
        }
    }

    private void WindowClosed(WindowFinder finder, AutomationElement eventElement, EventId eventId, int[] windowRunTimeId)
    {
        lock (_finderCreateLock)
        {
            RemoveWindowFinder(windowRunTimeId);
        }
    }

    private void RemoveWindowFinder(int[] windowRunTimeId)
    {
        var key = windowRunTimeId.ToWindowRunTimeId();
        if (WindowFinders.TryRemove(key, out var finder))
        {
            finder.Dispose();
            LogManager.Log($"window closed, finder removed [{key}]");
            MonitorHelper.Publish(SnapshotFinders());
        }
        else
        {
            LogManager.Log($"window closed, no finder for [{key}]");
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

    // Структурное сравнение по содержимому массива (record по умолчанию сравнивал бы int[] по ссылке).
    public virtual bool Equals(WindowRunTimeId other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (_runtimeId is null || other._runtimeId is null) return _runtimeId is null && other._runtimeId is null;
        if (_runtimeId.Length != other._runtimeId.Length) return false;
        for (int i = 0; i < _runtimeId.Length; i++)
            if (_runtimeId[i] != other._runtimeId[i]) return false;
        return true;
    }

    public override int GetHashCode()
    {
        if (_runtimeId is null) return 0;
        var hash = new HashCode();
        foreach (var id in _runtimeId) hash.Add(id);
        return hash.ToHashCode();
    }

    public override string ToString()
    {
        return _runtimeId == null? null : $"[{string.Join(",", _runtimeId)}]";
    }

    public static implicit operator string(WindowRunTimeId windowRunTimeId) => windowRunTimeId.ToString();
}