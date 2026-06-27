using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core.Identifiers;
using FlaUI.UIA3;
using System.Diagnostics;
using UIDriver;
class Program
{
    static UIA3Automation automation = null!;

    static async Task Main()
    {
        var processStartInfo = new ProcessStartInfo(@"C:\Program Files\Microsoft Office\root\Office16\EXCEL.EXE", "/e") { WindowStyle = ProcessWindowStyle.Normal };
        processStartInfo.UseShellExecute = false;
        var application = Application.Launch(processStartInfo);
        automation = new UIA3Automation();
        var mainWindow = application.GetMainWindow(automation);

        Console.ReadLine();

        var found = FindFirstRecursion(automation, mainWindow, cf =>
            cf.ByControlType(ControlType.DataItem).And(cf.ByAutomationId("A1")));

        Console.WriteLine(found?.Name);
        Console.ReadLine();

        var founds = FindAllRecursion(automation, mainWindow, cf =>
            cf.ByControlType(ControlType.DataItem).And(cf.ByAutomationId("A1")));

        foreach (var (el, i) in founds.Select((el, i) => (el, i)))
            Console.WriteLine($"[{i}] - {el.Name}");

        Console.ReadLine();
        automation.Dispose();
    }

    static AutomationElement? FindFirstRecursion(UIA3Automation automation, AutomationElement root, Func<ConditionFactory, ConditionBase> condition)
    {
        var selfCondition = condition(automation.ConditionFactory);
        var walker = automation.TreeWalkerFactory.GetCustomTreeWalker(selfCondition);
        var matcher = new PropertyMatcher(selfCondition);
        var stepsCount = 0;
        var stopwatch = Stopwatch.StartNew();

        AutomationElement? Search(AutomationElement node)
        {
            if (matcher.Matches(node)) return node;

            var child = walker.GetFirstChild(node);
            stepsCount++;
            while (child != null)
            {
                var found = Search(child);
                if (found != null) return found;

                child = walker.GetNextSibling(child);
                stepsCount++;
            }
            return null;
        }

        var result = Search(root);
        stopwatch.Stop();
        Console.WriteLine($"steps={stepsCount} time={stopwatch.Elapsed.TotalMilliseconds:F2}ms found={result != null}");
        return result;
    }

    static List<AutomationElement> FindAllRecursion(UIA3Automation automation, AutomationElement root, Func<ConditionFactory, ConditionBase> condition)
    {
        var selfCondition = condition(automation.ConditionFactory);
        var walker = automation.TreeWalkerFactory.GetCustomTreeWalker(selfCondition);
        var founds = new List<AutomationElement>();
        var stepsCount = 0;
        var stopwatch = Stopwatch.StartNew();

        void Search(AutomationElement node)
        {
            founds.Add(node);

            var child = walker.GetFirstChild(node);
            stepsCount++;
            while (child != null)
            {
                Search(child);

                child = walker.GetNextSibling(child);
                stepsCount++;
            }
        }

        Search(root);
        stopwatch.Stop();
        Console.WriteLine($"steps={stepsCount} time={stopwatch.Elapsed.TotalMilliseconds:F2}ms count={founds.Count}");
        return founds;
    }


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
        await elementFromDrvier.ClickAsync();

        ConditionFactory condFact = driver.ConditionFactory;
        var okButtonInFormatCellsBY = new BY 
        {
            SelfCondition = condFact.ByControlType(ControlType.Button).And(condFact.ByName("OK")),
            Parent = new BY 
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

public sealed class PropertyMatcher
{
    private readonly ConditionBase _condition;

    public PropertyMatcher(ConditionBase condition)
    {
        _condition = condition;
    }

    public bool Matches(AutomationElement element) => Matches(element, _condition);

    private bool Matches(AutomationElement element, ConditionBase condition) => condition switch
    {
        PropertyCondition propertyCondition => PropertyMatches(element, propertyCondition),
        AndCondition andCondition => andCondition.Conditions.All(child => Matches(element, child)),
        OrCondition orCondition => orCondition.Conditions.Any(child => Matches(element, child)),
        NotCondition notCondition => !Matches(element, notCondition.Condition),
        TrueCondition => true,
        FalseCondition => false,
        _ => throw new NotImplementedException($"Condition type {condition.GetType().Name} is not supported."),
    };

    private bool PropertyMatches(AutomationElement element, PropertyCondition propertyCondition)
    {
        var actual = GetPropertyValue(element, propertyCondition.Property);
        return EqualityComparer<object?>.Default.Equals(actual, propertyCondition.Value);
    }

    private object? GetPropertyValue(AutomationElement element, PropertyId propertyId)
    {
        try { return element.FrameworkAutomationElement.GetPropertyValue(propertyId); }
        catch { return null; }
    }
}