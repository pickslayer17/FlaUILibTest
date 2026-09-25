using System.Diagnostics;
using Microsoft.Extensions.Logging;
using UIDriver.Api;
using UIDriver.Diagnostics.Remote;

class Program
{
    static void Main()
    {
        var processStartInfo = new ProcessStartInfo(@"C:\Program Files\Microsoft Office\root\Office16\EXCEL.EXE", "/e")
        {
            WindowStyle = ProcessWindowStyle.Normal,
            UseShellExecute = false
        };

        using var loggerFactory = LoggerFactory.Create(builder => builder.AddSimpleConsole().SetMinimumLevel(LogLevel.Debug));
        using var visualizer = new RemoteSnapshotObserver(Path.Combine(AppContext.BaseDirectory, "UIDriver.Visualization.exe"));
        var driver = new Driver(new DriverOptions
        {
            TreeObservers = [visualizer],
            BranchObservers = [visualizer],
            LoggerFactory = loggerFactory
        });
        driver.Launch(processStartInfo);

        Console.WriteLine("\n=== press key ===");
        Console.ReadKey();

        driver.Dispose();
    }
}
