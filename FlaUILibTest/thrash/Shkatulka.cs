using FlaUI.Core;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using System.Diagnostics;
using UIDriver;

public static class Shkatulka
{
    public async static Task NewEngine()
    {
        var processStartInfo = new ProcessStartInfo(@"C:\Program Files\Microsoft Office\root\Office16\EXCEL.EXE", "/e")
        {
            WindowStyle = ProcessWindowStyle.Normal
        };
        processStartInfo.EnvironmentVariables["WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS"] = "--remote-debugging-port=9234";
        processStartInfo.UseShellExecute = false;
        var driver = new UIDriver.Driver();
        LogEventHandler.Subscribe(new ConsoleLogger());
        driver.Launch(processStartInfo);
        var elementFromDrvier = driver.Locator(cf => cf.ByControlType(ControlType.DataItem).And(cf.ByAutomationId("A1")));
        Console.ReadLine();
        await elementFromDrvier.ClickAsync();

        var buttonFileTab = driver.Locator(cf => cf.ByControlType(ControlType.Button)
        .And(cf.ByAutomationId("FileTabButton")));
        await buttonFileTab.ClickAsync();

        var backButton = driver.Locator(cf => cf.ByControlType(ControlType.ListItem)
        .And(cf.ByName("Back")));
        await backButton.ClickAsync();

        elementFromDrvier = driver.Locator(cf => cf.ByControlType(ControlType.DataItem).And(cf.ByAutomationId("B1")));
        await elementFromDrvier.ClickAsync();
        Console.ReadLine();

        ConditionFactory condFact = driver.ConditionFactory;
        var okButtonInFormatCellsBY = new BY
        {
            SelfCondition = condFact.ByControlType(ControlType.Button).And(condFact.ByName("OK")),
            IsChild = true,
            AncestorOrParent = new BY
            {
                Scope = WindowScope.Desktop,
                SelfCondition = condFact.ByControlType(ControlType.Window).And(condFact.ByName("Format Cells"))
            }
        };
        var okButtonInFormatCells = driver.Locator(okButtonInFormatCellsBY);
        await okButtonInFormatCells.ClickAsync();
        Console.WriteLine("works!");

        Console.ReadLine();
    }

    public async static Task OleEngineTest()
    {
        var processStartInfo = new ProcessStartInfo(@"C:\Program Files\Microsoft Office\root\Office16\EXCEL.EXE", "/e")
        {
            WindowStyle = ProcessWindowStyle.Normal
        };
        processStartInfo.EnvironmentVariables["WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS"] = "--remote-debugging-port=9234";
        processStartInfo.UseShellExecute = false;

        //MonitorApp.Start();

        var driver = new FlaUILibTest.UIDriver.UIDriver();
        driver.LaunchApplication(processStartInfo);

        var elementFromDrvier = driver.UILocator(cf => cf.ByControlType(ControlType.DataItem).And(cf.ByAutomationId("A1")));
        Console.WriteLine("works!");
        await elementFromDrvier.ClickAsync();

        var okButtonInFormatCells =
           driver.UILocator(
               cf => cf.ByControlType(ControlType.Button).And(cf.ByName("OK")),
               cf => cf.ByControlType(ControlType.Window).And(cf.ByName("Format Cells"))
               );
        await okButtonInFormatCells.ClickAsync();
    }

    //public async static void DcPushTest()
    //{
    //    // var test = new DcPushTest();

    //    // test.RunPreconditions();
    //    // await test.RunTestAsync();
    //    // test.CleanUp();
    //}
}
