using System.Diagnostics;
using FlaUI.Core;
using FlaUI.Core.Conditions;
using FlaUI.UIA3;

namespace UIDriver;

public sealed class Driver : IDisposable
{
    public ConditionFactory ConditionFactory => _automation.ConditionFactory;
    private readonly UIA3Automation _automation;
    private readonly ApplicationManager _applicationManager;
    private Application? _application;

    public Driver()
    {
        _automation = new UIA3Automation();
        _applicationManager = new ApplicationManager(_automation);
    }

    public void Launch(ProcessStartInfo processStartInfo)
    {
        _application = Application.Launch(processStartInfo);
        _applicationManager.ProcessId = _application.ProcessId;
        _applicationManager.RegisterDesktop(new AutomationElementObject(_automation.GetDesktop()));
        _applicationManager.RegisterDefault(new AutomationElementObject(_application.GetMainWindow(_automation)));
    }

    public Locator Locator(Func<ConditionFactory, ConditionBase> elementCondition)
    {
        var by = new BY { SelfCondition = elementCondition(_automation.ConditionFactory) };
        var locator = new Locator(by, _applicationManager);

        return locator;
    }

    public Locator Locator(BY by)
    {
        var locator = new Locator(by, _applicationManager);

        return locator;
    }

    public void Dispose()
    {
        _automation.Dispose();
    }
}
