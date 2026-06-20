using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Identifiers;
using FlaUI.UIA3;
using FlaUILibTest.Inspector;
using System.Diagnostics;

namespace FlaUILibTest.UIDriver;

public class UIDriver : IDisposable
{
    private Application _application;
    private UIA3Automation _automation;
    private List<WindowFinder> WindowFinders
    {
        get
        {
            return field;
        }
    } = new();
    private Window _rootWindow;
    private ConditionFactory CF;
    private SearchManager _searchManager = new SearchManager();

    public UIDriver()
    {
        _automation = new UIA3Automation();
        CF = _automation.ConditionFactory;
    }

    public void LaunchApplication(ProcessStartInfo processStartInfo)
    {
        _application = Application.Launch(processStartInfo);
        _rootWindow = _application.GetMainWindow(_automation);
        CreateWindowFinder(_rootWindow);
    }

    private void CreateWindowFinder(Window window)
    {
        var finder = new WindowFinder(window)
        {
            OnWindowEvent = WindowEventHandler,
            SearchFunc = _searchManager.FindFirst
        };
        WindowFinders.Add(finder);
        Log($"Window context created for window [{finder.Name}] with RuntimeId [{string.Join(",", finder.RootRuntimeId)}]");
    }

    private WindowFinder GetFinderByWindowRunTimeId(int[] runtimeId)
    {
        var finders = WindowFinders.Where(w => w.RootRuntimeId.SequenceEqual(runtimeId)).ToList();
        if (finders.Count != 1) throw new Exception("!!!pizdec");
        var finder = finders[0];

        return finder;
    }

    public UILocator UILocator(Func<ConditionFactory, ConditionBase> byFunc)
    {
        var by = byFunc(CF);

        // here we can add some logic to understand which window to use for search 

        var rootRuntimeId = GetWindowRuntimeId(_rootWindow);
        var finder = GetFinderByWindowRunTimeId(rootRuntimeId);

        return new UILocator(finder, by);
    }

    private int[] GetWindowRuntimeId(Window element) => element.Properties.RuntimeId.ValueOrDefault;

    private readonly Lock _windowLock = new();
    private void WindowEventHandler(WindowFinder finder, EventId eventId, AutomationElement eventElement, string eventName)
    {
        lock (_windowLock)
        {
            if (eventId.Equals(_automation.EventLibrary.Window.WindowOpenedEvent))
            {
                var runtimeId = string.Join(",", eventElement.Properties.RuntimeId.ValueOrDefault ?? []);
                var title = eventElement.Properties.Name.ValueOrDefault;
                Log($"window opened event" +
                    $" [{runtimeId}]:" +
                    $" Title = [{title}]");
                var success = WindowOpened(eventElement.AsWindow());
                Log(success ? $"Finder '{title}' created." : "something went wrong!!!");
            }
            else if (eventId.Equals(_automation.EventLibrary.Window.WindowClosedEvent))
            {
                Log($"window closed event");
                var success = WindowClosed(finder.RootRuntimeId);
                Log(success ? $"Finder {finder.Name} removed." : "something went wrong!!!");
            }
        }
    }

    private bool WindowOpened(Window window)
    {
        var runtimeId = window.Properties.RuntimeId.ValueOrDefault;
        if (WindowFinders.Any(w => w.RootRuntimeId.SequenceEqual(runtimeId)))
        {
            Log("ERROR!!!!Window already exists in driver!!!!!");
            return false;
        }
        CreateWindowFinder(window);
        return true;
    }

    private bool WindowClosed(int[] runtimeId)
    {
        var finder = GetFinderByWindowRunTimeId(runtimeId);
        var success = WindowFinders.Remove(finder);
        if (success)
        {
            finder.Dispose();
        }

        return success;
    }

    public void Dispose()
    {
        _automation.Dispose();
        // _windowFinders dispose finders
    }

    private void Log(string message, [System.Runtime.CompilerServices.CallerMemberName] string caller = "")
    {
        LogManager.Log($"DRIVER::{caller}", message);
    }
}
