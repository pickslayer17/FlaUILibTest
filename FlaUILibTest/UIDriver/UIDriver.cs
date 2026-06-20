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
    private WindowManager _windowManager;
    private Window _rootWindow;
    private ConditionFactory CF;

    public UIDriver()
    {
        _automation = new UIA3Automation();
        CF = _automation.ConditionFactory;
        _windowManager = new WindowManager(_automation);
    }

    public void LaunchApplication(ProcessStartInfo processStartInfo)
    {
        _application = Application.Launch(processStartInfo);
        _rootWindow = _application.GetMainWindow(_automation);
        _windowManager.CreateWindowFinder(_rootWindow);
    }

    public UILocator UILocator(Func<ConditionFactory, ConditionBase> byFunc)
    {
        var by = byFunc(CF);
        var finder = _windowManager.GetFinderByRuntimeId(_rootWindow.Properties.RuntimeId.ValueOrDefault);
        return new UILocator(finder, by);
    }

    public void Dispose()
    {
        _automation.Dispose();
    }
}