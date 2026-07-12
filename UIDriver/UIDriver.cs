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
        UIAutomationProvider.Automation = _automation;
        _applicationManager = new UIApplicationManager();
    }

    public void Launch(ProcessStartInfo processStartInfo)
    {
        _process = Process.Start(processStartInfo);
        _applicationManager.ProcessId = _process!.Id;

        var desktop = _automation.GetRootElement();
        _applicationManager.RegisterDesktop(new UIAutomationElement(desktop));

        var mainWindow = WaitForMainWindow(desktop);
        _applicationManager.RegisterDefault(new UIAutomationElement(mainWindow));
    }

    private IUIAutomationElement WaitForMainWindow(IUIAutomationElement desktop)
    {
        var processIdCondition = _automation.CreatePropertyCondition((int)UiaProperty.ProcessId, _process!.Id);

        IUIAutomationElement? window = null;
        for (var attempt = 0; attempt < 50 && window == null; attempt++)
        {
            window = desktop.FindFirst(TreeScope.TreeScope_Children, processIdCondition);
            if (window == null) Thread.Sleep(200);
        }

        return window!;
    }

    public UILocator Locator(UIBy by)
    {
        var locator = new UILocator(by, _applicationManager);

        return locator;
    }

    public void Dispose()
    {
    }
}
