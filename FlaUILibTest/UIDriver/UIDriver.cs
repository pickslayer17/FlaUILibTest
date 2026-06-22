using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.UIA3;
using FlaUILibTest.Constants;
using FlaUILibTest.Inspector;
using System.Diagnostics;

namespace FlaUILibTest.UIDriver;

public class UIDriver : IDisposable
{
    private Application _application;
    private UIA3Automation _automation;
    private FinderManager _windowManager;
    private AutomationElement _desktop;
    private Window _rootWindow;
    private ConditionFactory CF;

    public UIDriver()
    {
        _automation = new UIA3Automation();
        CF = _automation.ConditionFactory;
        _windowManager = new FinderManager();
    }

    public void LaunchApplication(ProcessStartInfo processStartInfo)
    {
        _application = Application.Launch(processStartInfo);
        _desktop = _automation.GetDesktop();
        _rootWindow = _application.GetMainWindow(_automation);
        _windowManager.CreateWindowFinder(_desktop, FinderTypes.Desktop);
        _windowManager.CreateWindowFinder(_rootWindow, FinderTypes.RootWindow);
    }

    public UILocator UILocator(Func<ConditionFactory, ConditionBase> elementCondition, Func<ConditionFactory, ConditionBase> windowCondition = null)
    {
        var elementBy = new BY(elementCondition(CF));
        var windowBy = windowCondition != null ? new BY(windowCondition(CF)) : null;

        var source = _windowManager.CreateSource(windowBy);
        return new UILocator(source, elementBy);
    }

    public void Dispose()
    {
        _automation.Dispose();
    }
}