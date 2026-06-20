using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Identifiers;
using FlaUI.UIA3;

namespace FlaUILibTest.Inspector;

public class WindowManager
{
    private readonly Lock _searchLock = new();
    private readonly UIA3Automation _automation;
    private readonly Lock _windowLock = new();

    private List<WindowFinder> WindowFinders
    {
        get
        {
            return field;
        }
    } = new();

    public WindowManager(UIA3Automation automation)
    {
        _automation = automation;
    }

    public WindowFinder CreateWindowFinder(Window window)
    {
        var finder = new WindowFinder(window, FindFirst)
        {
            OnWindowEvent = WindowEventHandler,
        };
        WindowFinders.Add(finder);
        Log($"Finder created for window [{finder.Name}] with RuntimeId [{string.Join(",", finder.RootRuntimeId)}]");
        return finder;
    }

    public WindowFinder GetFinderByRuntimeId(int[] runtimeId)
    {
        var finders = WindowFinders.Where(w => w.RootRuntimeId.SequenceEqual(runtimeId)).ToList();
        if (finders.Count != 1) throw new Exception($"RuntimeId [{string.Join(",", runtimeId)}] found in {finders.Count} finders!");
        return finders[0];
    }
    
    public AutomationElement FindFirst(AutomationElement root, ConditionBase condition)
    {
        lock (_searchLock)
        {
            return root.FindFirstDescendant(condition);
        }
    }

    private void WindowEventHandler(WindowFinder finder, EventId eventId, AutomationElement eventElement, string eventName)
    {
        lock (_windowLock)
        {
            if (eventId.Equals(_automation.EventLibrary.Window.WindowOpenedEvent))
            {
                var runtimeId = string.Join(",", eventElement.Properties.RuntimeId.ValueOrDefault ?? []);
                var title = eventElement.Properties.Name.ValueOrDefault;
                Log($"window opened [{runtimeId}]: Title = [{title}]");
                var success = WindowOpened(eventElement.AsWindow());
                Log(success ? $"Finder '{title}' created." : "window not processed");
            }
            else if (eventId.Equals(_automation.EventLibrary.Window.WindowClosedEvent))
            {
                Log($"window closed from finder [{finder.Name}]");
                var success = WindowClosed(finder.RootRuntimeId);
                Log(success ? $"Finder {finder.Name} removed." : "finder not found");
            }
        }
    }

    private bool WindowOpened(Window window)
    {
        var runtimeId = window.Properties.RuntimeId.ValueOrDefault;
        if (WindowFinders.Any(w => w.RootRuntimeId.SequenceEqual(runtimeId)))
        {
            Log("Window already exists");
            return false;
        }
        CreateWindowFinder(window);
        return true;
    }

    private bool WindowClosed(int[] runtimeId)
    {
        var finder = GetFinderByRuntimeId(runtimeId);
        var success = WindowFinders.Remove(finder);
        if (success)
        {
            finder.Dispose();
        }
        return success;
    }

    private void Log(string message, [System.Runtime.CompilerServices.CallerMemberName] string caller = "")
    {
        LogManager.Log($"WM::{caller}", message);
    }
}

