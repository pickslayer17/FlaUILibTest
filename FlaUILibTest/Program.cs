using System.Diagnostics;
using Microsoft.Extensions.Logging;
using UIDriver.Api;
using UIDriver.Visualization;

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
        var driver = new Driver(new DriverOptions
        {
            TreeObservers = [TreeVisualizer.Instance],
            BranchObservers = [BranchVisualizer.Instance],
            LoggerFactory = loggerFactory
        });
        driver.Launch(processStartInfo);

        Console.WriteLine("\n=== press key ===");
        Console.ReadKey();

        driver.Dispose();
    }
}
