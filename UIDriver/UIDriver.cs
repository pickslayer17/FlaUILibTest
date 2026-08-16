using System.Diagnostics;
using Interop.UIAutomationClient;
using UIDriver.Constants;

namespace UIDriver;

public sealed class UIDriver : IDisposable
{
    private readonly IUIAutomation _automation;
    private readonly UIApplicationManager _applicationManager;
    private Process? _process;

    public UIDriver()
    {
        _automation = new CUIAutomation8();
        _applicationManager = new UIApplicationManager(_automation);
    }

    public void Launch(ProcessStartInfo processStartInfo)
    {
        _process = Process.Start(processStartInfo);
        _applicationManager.ProcessId = _process!.Id;

        var desktop = _automation.GetRootElement();
        var processIdCondition = _automation.CreatePropertyCondition((int)UiaProperty.ProcessId, _process.Id);
        var mainWindow = desktop.FindFirst(TreeScope.TreeScope_Children, processIdCondition);

        _applicationManager.RegisterDefault(new UIAutomationElement(mainWindow));
    }

    public UILocator Locator(UIBy by)
    {
        var locator = new UILocator(by, _applicationManager);

        return locator;
    }

    public void PrintCollectedTreesParents()
    {
        _applicationManager.PrintCollectedTreesParents();
    }

    public void Dispose()
    {
    }
}
