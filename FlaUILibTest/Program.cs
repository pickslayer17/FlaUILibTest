using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core.Identifiers;
using FlaUI.UIA3;
using FlaUILibTest;
using FlaUILibTest.DcPushBenchMark;
using FlaUILibTest.Inspector;
using System.Diagnostics;
using System.Security.Cryptography;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

class Program
{
    static async Task Main()
    {
        //var driver = new UIDriver();
        //var window = driver.LaunchApplication(processStartInfo);

        //await driver.Locator(By.Tab("Insert")).ClickAsync();
        //await driver.Locator(By.Button("Table")).ClickAsync();

        //driver.SwitchTo(By.Window("Create Table"));
        //await driver.Locator(By.Button("OK")).ClickAsync();

        //driver.SwitchToMainContent();
        var processStartInfo = new ProcessStartInfo(@"C:\Program Files\Microsoft Office\root\Office16\EXCEL.EXE", "/e")
        {
            WindowStyle = ProcessWindowStyle.Normal
        };
        processStartInfo.EnvironmentVariables["WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS"] = "--remote-debugging-port=9234";
        processStartInfo.UseShellExecute = false;
        var application = Application.Launch(processStartInfo);
        var automation = new UIA3Automation();
        var window = application.GetMainWindow(automation);
        var cf = automation.ConditionFactory;
        var finder = new ModuleFinder(window);

        var cellA1 = await finder.RegisterAndGetElementAsync(
            cf.ByControlType(ControlType.DataItem).And(cf.ByName("A1"))
        );

        for (int i = 0; i < 5; i++)
        {
            cellA1.Click();
            Console.WriteLine($"Click {i}");
            for (int j = 10; j > 0; j--)
            {
                Console.WriteLine($"waiting {j}");
                await Task.Delay(200);
            }
           
        }







        //var test = new DcPushTest();

        //test.RunPreconditions();
        //await test.RunTestAsync();
        //test.CleanUp();

        //Console.OutputEncoding = System.Text.Encoding.UTF8;

        //var psi = new ProcessStartInfo(@"C:\Program Files\Microsoft Office\root\Office16\EXCEL.EXE", "/e")
        //{
        //    WindowStyle = ProcessWindowStyle.Normal
        //};
        //var application = Application.Launch(psi);
        //var automation = new UIA3Automation();
        //var window = application.GetMainWindow(automation);
        //var cf = automation.ConditionFactory;

        //var tree = new UITree(window);
        //tree.WatchElement(conditionFactory.ByControlType(ControlType.DataItem).And(conditionFactory.ByName("A1")));
        //tree.SubscribeToEvents(window);
        //await tree.BuildAsync();

        //var finder = new ModuleFinder();
        //finder.Subscribe(window);
        //var cellA1Task = finder.Register(cf.ByControlType(ControlType.DataItem).And(cf.ByName("A1")));
        //var cellA1 = await cellA1Task;
        //cellA1.Click();




        //Console.WriteLine( "wait for blank to grid action");
        //Console.ReadLine();
        //window.RegisterPropertyChangedEvent(TreeScope.Subtree, (el, propId, val) =>
        //{
        //    Console.WriteLine($"--- WINDOW DIRECT: {propId.Name} = {val}");
        //},
        //    PropertyId.Register(AutomationType.UIA3, 30003, "Name"),
        //    PropertyId.Register(AutomationType.UIA3, 30010, "IsEnabled"),
        //    PropertyId.Register(AutomationType.UIA3, 30005, "BoundingRectangle")
        //);

        //window.RegisterStructureChangedEvent(TreeScope.Subtree, (el, changeType, rid) =>
        //{
        //    var name = "";
        //    var cls = "";
        //    try { name = el.Properties.Name.ValueOrDefault; } catch { }
        //    try { cls = el.Properties.ClassName.ValueOrDefault; } catch { }
        //    Console.WriteLine($"--- WINDOW STRUCTURE: {changeType} | {name} | {cls}");
        //});

        //Console.WriteLine("window subscribed. Click cells...");
        //Console.ReadLine();

        // EventManagerExtended.Instance.SubscribeAll(window);

        //// 1. Подписка
        //EventManager.Instance.Subscribe(window);

        //// 2. Модуль — якорь XLDESK
        //var gridModule = new Module(window, conditionFactory.ByClassName("XLDESK"));

        //// 4. Элемент — ячейка A1
        //var cellA1 = new Element(gridModule,
        //   conditionFactory.ByControlType(ControlType.DataItem).And(conditionFactory.ByName("A1")));

        //await cellA1.ClickAsync();


        Console.ReadLine();
    }

    static AutomationElement GetElement(AutomationElement root, ConditionBase condition, int timeoutMs = 10000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            var found = root.FindFirstDescendant(condition);
            if (found != null)
                return found;
            Thread.Sleep(200);
        }
        throw new TimeoutException($"Element not found within {timeoutMs}ms");
    }

    static AutomationElement GetElementByXPath2(AutomationElement root, string xpath, int timeoutMs = 10000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            var found = root.FindFirstByXPath(xpath);
            if (found != null)
                return found;
            Thread.Sleep(200);
        }
        throw new TimeoutException($"Element not found within {timeoutMs}ms");
    }

    static AutomationElement GetElementByXPath(AutomationElement root, string xpath, int timeoutMs = 10000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                if (root.FindAllByXPath(xpath).Length != 0)
                {
                    var element = root.FindFirstByXPath(xpath);
                    return element;
                }
            }
            catch { }
            Thread.Sleep(200);
        }
        throw new TimeoutException($"Element not found within {timeoutMs}ms");
    }
}
