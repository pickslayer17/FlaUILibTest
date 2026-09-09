using System.Diagnostics;

class Program
{
    static void Main()
    {
        var processStartInfo = new ProcessStartInfo(@"C:\Program Files\Microsoft Office\root\Office16\EXCEL.EXE", "/e")
        {
            WindowStyle = ProcessWindowStyle.Normal,
            UseShellExecute = false
        };

        var driver = new UIDriver.UIDriver();
        driver.Launch(processStartInfo);

        Console.WriteLine("\n=== press key ===");
        Console.ReadKey();

        driver.Dispose();
    }
}
