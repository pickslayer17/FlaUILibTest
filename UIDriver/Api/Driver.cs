using System.Diagnostics;
using UIDriver.Diagnostics;
using UIDriver.Exceptions;
using UIDriver.Uia;
using UIDriver.Uia.Constants;
using UIDriver.Windows;

namespace UIDriver.Api;

public sealed class Driver : IDisposable
{
    private readonly UiaAutomation _automation;
    private readonly UIApplicationManager _applicationManager;
    private Process? _process;

    public Driver() : this(new DriverOptions())
    {
    }

    public Driver(DriverOptions options)
    {
        _automation = new UiaAutomation();
        var snapshotPublisher = new SnapshotPublisher(options.TreeObservers, options.BranchObservers);
        _applicationManager = new UIApplicationManager(_automation, snapshotPublisher, options.LoggerFactory);
    }

    public void Launch(ProcessStartInfo processStartInfo)
    {
        _process = Process.Start(processStartInfo);
        _applicationManager.ProcessId = _process!.Id;

        var mainWindow = _automation.Desktop.Live.FindFirstChild(UiaProperty.ProcessId, _process.Id)
            ?? throw new WindowNotFoundException($"Main window of process {_process.Id} not found.");

        _applicationManager.RegisterDefault(mainWindow);
    }

    public UILocator Locator(UIBy by)
    {
        var locator = new UILocator(by, _applicationManager);

        return locator;
    }

    public void Dispose() => _applicationManager.Dispose();
}
