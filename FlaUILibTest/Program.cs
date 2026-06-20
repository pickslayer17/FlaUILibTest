using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core.Identifiers;
using FlaUI.UIA3;
using FlaUILibTest;
using FlaUILibTest.DcPushBenchMark;
using FlaUILibTest.Inspector;
using FlaUILibTest.UIDriver;
using System.Diagnostics;
using Interop.UIAutomationClient;

class Program
{
    static async Task Main()
    {
        var processStartInfo = new ProcessStartInfo(@"C:\Program Files\Microsoft Office\root\Office16\EXCEL.EXE", "/e")
        {
            WindowStyle = ProcessWindowStyle.Normal
        };
        processStartInfo.EnvironmentVariables["WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS"] = "--remote-debugging-port=9234";
        processStartInfo.UseShellExecute = false;

        var driver = new UIDriver();
        driver.LaunchApplication(processStartInfo);

        var elementFromDrvier = driver.UILocator(cf => cf.ByControlType(ControlType.DataItem).And(cf.ByName("A1")));
        Console.WriteLine("works!");
        await elementFromDrvier.ClickAsync();


        //var test = new DcPushTest();

        //test.RunPreconditions();
        //await test.RunTestAsync();
        //test.CleanUp();

        //Console.ReadLine();
        //Console.WriteLine("started...");

        //var uia = new CUIAutomationClass();

        //// получаем root element
        //var root = uia.GetRootElement();

        //// находим окно Excel
        //var condition = uia.CreatePropertyCondition(30005, "Book1 - Excel"); // 30005 = Name
        //var excelWindow = root.FindFirst(Interop.UIAutomationClient.TreeScope.TreeScope_Descendants, condition);

        //// подписываемся на WindowClosed
        //uia.AddAutomationEventHandler(
        //    20017, // WindowClosedEvent ID
        //    excelWindow,
        //    Interop.UIAutomationClient.TreeScope.TreeScope_Subtree,
        //    null, // no cache
        //    new WindowClosedHandler()
        //);

        //Console.WriteLine("subscribed...");


        Console.ReadLine();
    }

}

public class WindowClosedHandler : IUIAutomationEventHandler
{
    public void HandleAutomationEvent(IUIAutomationElement sender, int eventId)
    {
        // sender — элемент который стрельнул
        // пробуем снять runtimeId
        try
        {
            var rid = sender.GetRuntimeId();
            Console.WriteLine($"CLOSED: rid=[{string.Join(",", rid)}]");
        }
        catch
        {
            Console.WriteLine("CLOSED: sender dead");
        }

        try
        {
            Console.WriteLine($"CLOSED: name={sender.CurrentName}");
        }
        catch
        {
            Console.WriteLine("CLOSED: name dead");
        }
    }
}