using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.UIA3;
using FlaUILibTest.Inspector;
using System.Diagnostics;

namespace FlaUILibTest.UIDriver;

public class UIDriver : IDisposable
{
    private Application _application;
    private UIA3Automation _automation;
    private Dictionary<Window, ModuleFinder> _windowFinders = new();
    private Window _rootWindow;
    private ModuleFinder _currentFinder;

    public List<Window> ApplicationWindows
    {
        get
        {
            return new List<Window>(_windowFinders.Keys);
        }
    }
    public ConditionFactory CF;

    public UIDriver()
    {
        _automation = new UIA3Automation();
        CF = _automation.ConditionFactory;
    }


    public void LaunchApplication(ProcessStartInfo processStartInfo)
    {
        _application = Application.Launch(processStartInfo);
        _rootWindow = _application.GetMainWindow(_automation);
        var moduleFinder = new ModuleFinder(_rootWindow);
        _windowFinders.TryAdd(_rootWindow, moduleFinder);
        _currentFinder = moduleFinder;
    }

    public UILocator UILocator(ConditionBase by)
    {
        return new UILocator(_currentFinder, by);
    }

    public bool SwitchToWindow(Window window)
    {
        if (_windowFinders.TryGetValue(window, out var finder))
        {
            _currentFinder = finder;

            return true;
        }

        return false;
    }

    private bool WindowOpened(Window window)
    {
        if (!_windowFinders.ContainsKey(window))
        {
            return _windowFinders.TryAdd(window, new ModuleFinder(window));
        }
        else
        {
            return false;
        }
    }

    private bool WindowClosed(Window window)
    {
        // here is very complex logic - dont want to spend time on it

        if (window == _rootWindow)
        {
            // exception ok kill everything
        }

        if (!_windowFinders.ContainsKey(window))
        {
            if (_windowFinders.TryGetValue(window, out var finder) )
            {
                if (finder == _windowFinders[_rootWindow])
                {
                    throw new Exception("somthing went completely wrong");
                } 
                else if (finder == _currentFinder)
                {
                    _currentFinder = _windowFinders[_rootWindow];
                }
                else
                {
                }
                //finder.dispose
            }
            
            return _windowFinders.Remove(window);
        }
        else
        {
            return false;
        }
    }

    public void Dispose()
    {
        _automation.Dispose();
        // _windowFinders dispose finders
    }
}
