using System.Diagnostics;
using FlaUI.Core;
using FlaUI.Core.Conditions;
using FlaUI.UIA3;

namespace UIDriver;

public sealed class Driver : IDisposable
{
    private readonly UIA3Automation _automation = new();
    private readonly ApplicationManager _applicationManager = new();
    private Application? _application;

    public void Launch(ProcessStartInfo processStartInfo)
    {
        _application = Application.Launch(processStartInfo);
        _applicationManager.RegisterDesktop(new AutomationElementObject(_automation.GetDesktop()));
        _applicationManager.RegisterDefault(new AutomationElementObject(_application.GetMainWindow(_automation)));
    }

    public Locator Locator(Func<ConditionFactory, ConditionBase> element)
        => new(new BY { SelfCondition = element(_automation.ConditionFactory) }, _applicationManager);

    public void Dispose() => _automation.Dispose();
}
