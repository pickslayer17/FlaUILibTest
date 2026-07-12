using System.Diagnostics;
using FlaUI.Core;
using FlaUI.Core.Conditions;
using FlaUI.UIA3;

namespace UIDriver;

public sealed class UIDriver : IDisposable
{
    public ConditionFactory ConditionFactory => _automation.ConditionFactory;
    private UIA3Automation _automation;
    private readonly UIApplicationManager _applicationManager;
    private Application? _application;

    public UIDriver()
    {
        _automation = new UIA3Automation();
        _applicationManager = new UIApplicationManager();
    }

    public void Launch(ProcessStartInfo processStartInfo)
    {
        _application = Application.Launch(processStartInfo);
        _applicationManager.ProcessId = _application.ProcessId;
        _applicationManager.RegisterDesktop(new UIAutomationElement(_automation.GetDesktop()));
        _applicationManager.RegisterDefault(new UIAutomationElement(_application.GetMainWindow(_automation)));
    }

    public UILocator Locator(Func<ConditionFactory, ConditionBase> elementCondition)
    {
        var by = new UIBy { SelfCondition = elementCondition(_automation.ConditionFactory) };
        var locator = new UILocator(by, _applicationManager);

        return locator;
    }

    public UILocator Locator(UIBy by)
    {
        var locator = new UILocator(by, _applicationManager);

        return locator;
    }

    public void Dispose()
    {
        _automation.Dispose();
    }
}
