using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Capturing;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core.Identifiers;
using FlaUI.UIA3;
using FlaUI.UIA3.Converters;
using FlaUILibTest.Extensions;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
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

        
        Console.WriteLine("\n=== press key ===");

        var nativeAutomation = automation.NativeAutomation;
        var nativeRoot = ((UIA3FrameworkAutomationElement)mainWindow.FrameworkAutomationElement).NativeElement;

        var closeCondition = automation.ConditionFactory.ByControlType(ControlType.Button).And(automation.ConditionFactory.ByName("Close"));
        var a1Condition = automation.ConditionFactory.ByControlType(ControlType.DataItem).And(automation.ConditionFactory.ByAutomationId("A1"));
        var scrollBarCondition = automation.ConditionFactory.ByControlType(ControlType.ScrollBar).And(automation.ConditionFactory.ByClassName("NetUIScrollBar"));
        var targetCondition = a1Condition;

        Console.ReadLine();
        Console.WriteLine("\n=== CACHED SEARCH ===");
        CachedSearch(mainWindow, targetCondition);
        
        Console.ReadLine();
        Console.WriteLine("\n=== HYBRID SEARCH ===");
        HybridSearchFind(mainWindow, targetCondition);
        HybridSearchFind(mainWindow, targetCondition, true);

        var convertedCondition = ConditionConverter.ToNative(automation, targetCondition);
        var nativeMatcher = new NativePropertyMatcher(targetCondition).Matches;
        Func<Interop.UIAutomationClient.IUIAutomationElement, object?> describeNative = element => 
        { 
            string name = "";
            bool? isEnabled = null;
            try { name = element.CurrentName; } catch { name = "not supported";  }; 
            try { isEnabled = element.CurrentIsEnabled != 0; } catch {  } 
            var enabled = isEnabled.HasValue ? isEnabled.Value ? "true" : "false" : "not supported";

            return $"name = '{name}'. isEnabled = '{enabled}'";
        };

        TreeSearch<Interop.UIAutomationClient.IUIAutomationTreeWalker, Interop.UIAutomationClient.IUIAutomationElement> NativeSearch(string label, Interop.UIAutomationClient.IUIAutomationTreeWalker walker)
            => new(label, walker.GetFirstChildElement, walker.GetNextSiblingElement, nativeMatcher, describeNative);

        var flaUiMatcher = new PropertyMatcher(targetCondition).Matches;
        Func<AutomationElement, object?> describeFlaUi = element => { try { return element.Name; } catch { return null; } };

        TreeSearch<ITreeWalker, AutomationElement> FlaUiSearch(string label, ITreeWalker walker)
            => new(label, walker.GetFirstChild, walker.GetNextSibling, flaUiMatcher, describeFlaUi);

        // CASE 1: native CreateTreeWalker(RawViewCondition)
        Console.WriteLine("\n=== [1] native CreateTreeWalker(RawViewCondition) ===");
        NativeSearch("[1] native Create+RawViewCondition", nativeAutomation.CreateTreeWalker(nativeAutomation.RawViewCondition)).FindFirst(nativeRoot);
        NativeSearch("[1] native Create+RawViewCondition", nativeAutomation.CreateTreeWalker(nativeAutomation.RawViewCondition)).FindAll(nativeRoot);

        // CASE 2: native CreateTreeWalker(ControlViewCondition)
        Console.WriteLine("\n=== [2] native CreateTreeWalker(ControlViewCondition) ===");
        NativeSearch("[2] native Create+ControlViewCondition", nativeAutomation.CreateTreeWalker(nativeAutomation.ControlViewCondition)).FindFirst(nativeRoot);
        NativeSearch("[2] native Create+ControlViewCondition", nativeAutomation.CreateTreeWalker(nativeAutomation.ControlViewCondition)).FindAll(nativeRoot);

        // CASE 3: native CreateTreeWalker(ContentViewCondition)
        Console.WriteLine("\n=== [3] native CreateTreeWalker(ContentViewCondition) ===");
        NativeSearch("[3] native Create+ContentViewCondition", nativeAutomation.CreateTreeWalker(nativeAutomation.ContentViewCondition)).FindFirst(nativeRoot);
        NativeSearch("[3] native Create+ContentViewCondition", nativeAutomation.CreateTreeWalker(nativeAutomation.ContentViewCondition)).FindAll(nativeRoot);

        // CASE 4: native CreateTreeWalker(converted targetCondition)
        Console.WriteLine("\n=== [4] native CreateTreeWalker(convertedCondition) ===");
        NativeSearch("[4] native Create+convertedCondition", nativeAutomation.CreateTreeWalker(convertedCondition)).FindFirst(nativeRoot);
        NativeSearch("[4] native Create+convertedCondition", nativeAutomation.CreateTreeWalker(convertedCondition)).FindAll(nativeRoot);

        // CASE 5: native RawViewWalker (no condition)
        Console.WriteLine("\n=== [5] native RawViewWalker ===");
        NativeSearch("[5] native RawViewWalker", nativeAutomation.RawViewWalker).FindFirst(nativeRoot);
        NativeSearch("[5] native RawViewWalker", nativeAutomation.RawViewWalker).FindAll(nativeRoot);

        // CASE 6: native ControlViewWalker (no condition)

        Console.WriteLine("\n=== [6] native ControlViewWalker ===");
        NativeSearch("[6] native ControlViewWalker", nativeAutomation.ControlViewWalker).FindFirst(nativeRoot);
        NativeSearch("[6] native ControlViewWalker", nativeAutomation.ControlViewWalker).FindAll(nativeRoot);

        // CASE 7: native ContentViewWalker (no condition)
        Console.WriteLine("\n=== [7] native ContentViewWalker ===");
        NativeSearch("[7] native ContentViewWalker", nativeAutomation.ContentViewWalker).FindFirst(nativeRoot);
        NativeSearch("[7] native ContentViewWalker", nativeAutomation.ContentViewWalker).FindAll(nativeRoot);

        // CASE 8: FlaUI RawViewWalker
        Console.WriteLine("\n=== [8] FlaUI RawViewWalker ===");
        FlaUiSearch("[8] FlaUI RawViewWalker", automation.TreeWalkerFactory.GetRawViewWalker()).FindFirst(mainWindow);
        FlaUiSearch("[8] FlaUI RawViewWalker", automation.TreeWalkerFactory.GetRawViewWalker()).FindAll(mainWindow);

        // CASE 9: FlaUI ControlViewWalker
        Console.WriteLine("\n=== [9] FlaUI ControlViewWalker ===");
        FlaUiSearch("[9] FlaUI ControlViewWalker", automation.TreeWalkerFactory.GetControlViewWalker()).FindFirst(mainWindow);
        FlaUiSearch("[9] FlaUI ControlViewWalker", automation.TreeWalkerFactory.GetControlViewWalker()).FindAll(mainWindow);

        // CASE 10: FlaUI ContentViewWalker
        Console.WriteLine("\n=== [10] FlaUI ContentViewWalker ===");
        FlaUiSearch("[10] FlaUI ContentViewWalker", automation.TreeWalkerFactory.GetContentViewWalker()).FindFirst(mainWindow);
        FlaUiSearch("[10] FlaUI ContentViewWalker", automation.TreeWalkerFactory.GetContentViewWalker()).FindAll(mainWindow);

        // CASE 11: FlaUI CustomTreeWalker(targetCondition) — главный кандидат
        Console.WriteLine("\n=== [11] FlaUI GetCustomTreeWalker(targetCondition) ===");
        FlaUiSearch("[11] FlaUI GetCustomTreeWalker", automation.TreeWalkerFactory.GetCustomTreeWalker(targetCondition)).FindFirst(mainWindow);
        FlaUiSearch("[11] FlaUI GetCustomTreeWalker", automation.TreeWalkerFactory.GetCustomTreeWalker(targetCondition)).FindAll(mainWindow);

        Leaderboard.PrintResults();

        automation.Dispose();
    }

    static List<AutomationElement> HybridSearchFind(AutomationElement root, ConditionBase condition, bool findAll = false)
    {
        var conditionWalker = automation.TreeWalkerFactory.GetCustomTreeWalker(condition);
        var rawWalker = automation.TreeWalkerFactory.GetRawViewWalker();
        var windowRuntimeId = SafeRunTimeId(root);
        var desktopRuntimeId = SafeRunTimeId(automation.GetDesktop());
        var windowProcessId = SafeProcessId(root);
        int stepsCount = 0;

        var stopwatch = Stopwatch.StartNew();

        var founds = new List<AutomationElement>();
        var node = conditionWalker.GetFirstChild(root);
        stepsCount++;

        if(findAll == false)
        {
            stopwatch.Stop();
            Console.WriteLine($"[FindFirst] time={stopwatch.Elapsed.TotalMilliseconds:F2}ms found={node != null} steps={stepsCount}");
            
            Leaderboard.ReportFindFirst("[12] FlaUI HybridTreeWalker", stopwatch.Elapsed.TotalMilliseconds, node != null, stepsCount);
            return new List<AutomationElement> {node};
        }

        while (node != null)
        {
            // stepsCount++; because its just a check
            if (conditionWalker.GetFirstChild(node) != null) throw new Exception("oppa nihuya sebe!...");

            if (!IsPresentInWindow(rawWalker, node, windowProcessId, windowRuntimeId, desktopRuntimeId, ref stepsCount)) break;

            founds.Add(node);
            node = conditionWalker.GetNextSibling(node);
            stepsCount++;
        }

        stopwatch.Stop();

        Console.WriteLine($"[FindAll] time={stopwatch.Elapsed.TotalMilliseconds:F2}ms count={founds.Count} steps={stepsCount}");
        foreach (var (element, index) in founds.Select((element, index) => (element, index)))
            Console.WriteLine($"[{index}] - {SafeName(element)} {SafeRunTimeId(element).ToFormattedString()}");

        Leaderboard.ReportFindAll("[12] FlaUI HybridTreeWalker", stopwatch.Elapsed.TotalMilliseconds, founds.Count, stepsCount);

        return founds;
    }

    static bool IsPresentInWindow(ITreeWalker rawWalker, AutomationElement element, int windowProcessId, int[] windowRuntimeId, int[] desktopRuntimeId, ref int count)
    {
        var elementProcessId = SafeProcessId(element);
        count++;
        if(elementProcessId == 0)  throw new Exception("oppa nihuya sebe! element bez ProcessId");
        if (elementProcessId != windowProcessId) return false;

        var parent = rawWalker.GetParent(element);
        count++;
        while (parent != null)
        {
            var parentRuntimeId = SafeRunTimeId(parent);
            if (parentRuntimeId.SequenceEqual(windowRuntimeId)) return true;
            if (parentRuntimeId.SequenceEqual(desktopRuntimeId)) return false;

            parent = rawWalker.GetParent(parent);
            count++;
        }

        throw new Exception("oppa nihuya sebe! desktop proeban, parent == null");
    }

    static List<AutomationElement> CachedSearch(AutomationElement root, ConditionBase condition)
    {
        AutomationElement mainWindowCached = null;
        var mainWindowCondition = automation.ConditionFactory
        .ByControlType(ControlType.Window)
        .And(automation.ConditionFactory
            .ByName("Book1 - Excel")
            .Or(automation.ConditionFactory
                .ByClassName("XLMAIN")	
                )
            );
          
        ExecuteInCacheMode(() =>
        {
             mainWindowCached = root.FindFirst(TreeScope.Subtree, mainWindowCondition);
        }, message: "Request of cashed window");

        ExecuteInCacheMode(() =>
        {
             var a = root.FindAllDescendants(condition);
        }, message: "find directly");
        ExecuteInCacheMode(() =>
        {
             var a = root.FindAllDescendants(condition);
        }, message: "find directly with filter", treeFilter: condition);
        
        ExecuteInCacheMode(() =>
        {
            var treeOutput = new System.Text.StringBuilder();
             PrintCachedTree(mainWindowCached, 0, treeOutput);
             //System.Console.WriteLine(treeOutput);
             Console.WriteLine($"===NODES COUNT = [{printCachedTreeSteps}]===");
        }, message: "Cached tree built");

        ExecuteInCacheMode(() =>
        {
             var snapshot = BuildCachedNode(mainWindowCached, null);
        }, message: "cached node tree built");
        

        var result  = new List<AutomationElement>();
        ExecuteInCacheMode(() =>
        {
             FindInCache(mainWindowCached, condition, result);
        }, message: "Find in cache");
        
        System.Console.WriteLine($"Whole time in for cache actions: {cacheWholeSearchTime:f2}ms");
        Leaderboard.ReportFindAll("[13] CachedSearch", cacheWholeSearchTime, result.Count, 0);

        return result;
    }

    static double cacheWholeSearchTime = 0;

    static void ExecuteInCacheMode(Action action, ConditionBase treeFilter = null, string message = null)
    {
        var cacheWatch = Stopwatch.StartNew();
        CacheRequest cacheRequest;
        cacheRequest = new CacheRequest
        {
            TreeScope = TreeScope.Subtree,
            AutomationElementMode = AutomationElementMode.Full
        };
        if(treeFilter!= null)
            cacheRequest.TreeFilter = treeFilter;
        
        cacheRequest.Add(automation.PropertyLibrary.Element.ProcessId);
        cacheRequest.Add(automation.PropertyLibrary.Element.RuntimeId);
        cacheRequest.Add(automation.PropertyLibrary.Element.Name);
        cacheRequest.Add(automation.PropertyLibrary.Element.ControlType);
        cacheRequest.Add(automation.PropertyLibrary.Element.AutomationId);

        using (cacheRequest.Activate())
        {
            action.Invoke();
        }
      
        cacheWatch.Stop();
        cacheWholeSearchTime+=cacheWatch.Elapsed.TotalMilliseconds;
        Console.WriteLine($"[ExecuteInCacheMode] '{message}' completed. Time: {cacheWatch.Elapsed.TotalMilliseconds:F2}ms");
    }

    static void FindInCache(AutomationElement cache, ConditionBase condition, List<AutomationElement> result)
    {
        if(new PropertyMatcher(condition).Matches(cache)) result.Add(cache);

        foreach (var child in cache.CachedChildren)
        {
            FindInCache(child, condition, result);
        }
    }

    static CachedNode BuildCachedNode(AutomationElement element, CachedNode? parent)
    {
        var node = new CachedNode
        {
            Parent = parent,
            Name = SafeName(element),
            ProcessId = SafeProcessId(element),
            RuntimeId = SafeRunTimeId(element)
        };

        foreach (var child in element.CachedChildren)
            node.Children.Add(BuildCachedNode(child, node));

        return node;
    }

    static int printCachedTreeSteps = 0;
    static void PrintCachedTree(AutomationElement element, int depth, System.Text.StringBuilder output)
    {
        printCachedTreeSteps++;
        var name = SafeName(element);
        var processId = SafeProcessId(element);
        var runtimeId = SafeRunTimeId(element).ToFormattedString();

        output.AppendLine($"{new string(' ', depth * 2)}name='{name}' pid={processId} runtimeId={runtimeId}");

        foreach (var child in element.CachedChildren)
            PrintCachedTree(child, depth + 1, output);
    }

    static int[] SafeRunTimeId(AutomationElement element)
    {
        try { return element.Properties.RuntimeId.Value; }
        catch { return new int[0]; }
    }

    static int SafeProcessId(AutomationElement element)
    {
        try { return element.Properties.ProcessId.Value; }
        catch(Exception ex) { throw new Exception("oppa nihuya sebe! element bez ProcessId", ex); }
    }

    static object? SafeName(AutomationElement element)
    {
        try { return element.Name; }
        catch { return null; }
    }

    public sealed class CachedNode
    {
        public CachedNode? Parent { get; set; }
        public List<CachedNode> Children { get; } = new();
        public object? Name { get; set; }
        public int ProcessId { get; set; }
        public int[] RuntimeId { get; set; } = [];
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

public static class Leaderboard
{
    private static readonly List<(string Label, double Time, int Count, int steps)> _findFirst = new();
    private static readonly List<(string Label, double Time, int Count, int steps)> _findAll = new();

    public static void ReportFindFirst(string label, double time, bool found, int steps)
    {
        _findFirst.Add((label, time, found ? 1 : 0, steps));
    }

    public static void ReportFindAll(string label, double time, int count, int steps)
    {
        _findAll.Add((label, time, count, steps));
    }

    public static void PrintResults()
    {
        Print("FindFirst", _findFirst);
        Print("FindAll", _findAll);
    }

    private static void Print(string title, List<(string Label, double Time, int Count, int steps)> results)
    {
        var sorted = results.OrderBy(result => result.Count == 0).ThenBy(result => result.Time).ToList();
        var labelWidth = sorted.Count == 0 ? 0 : sorted.Max(result => result.Label.Length);

        Console.WriteLine($"\n========== {title} ==========");
        foreach (var (item, rank) in sorted.Select((item, index) => (item, index + 1)))
            Console.WriteLine($"[{rank,2}]  {item.Label.PadRight(labelWidth)}  {item.Time,8:F2}ms  count={item.Count}, steps = {item.steps}");
    }
}

public sealed class TreeSearch<TWalker, TElement> where TElement : class
{
    private readonly string _label;
    private readonly Func<TElement, TElement?> _getFirstChild;
    private readonly Func<TElement, TElement?> _getNextSibling;
    private readonly Func<TElement, bool> _matches;
    private readonly Func<TElement, object?> _describe;

    public TreeSearch(
        string label,
        Func<TElement, TElement?> getFirstChild,
        Func<TElement, TElement?> getNextSibling,
        Func<TElement, bool> matches,
        Func<TElement, object?> describe)
    {
        _label = label;
        _getFirstChild = getFirstChild;
        _getNextSibling = getNextSibling;
        _matches = matches;
        _describe = describe;
    }

    public TElement? FindFirst(TElement root)
    {
        var stepsCount = 0;
        var stopwatch = Stopwatch.StartNew();

        TElement? Search(TElement node)
        {
            if (_matches(node)) return node;

            var child = _getFirstChild(node);
            stepsCount++;
            while (child != null)
            {
                var found = Search(child);
                if (found != null) return found;

                child = _getNextSibling(child);
                stepsCount++;
            }
            return null;
        }

        var result = Search(root);
        stopwatch.Stop();
        Console.WriteLine($"[FindFirst] time={stopwatch.Elapsed.TotalMilliseconds:F2}ms found={result != null} steps={stepsCount}");
        if (result != null) Console.WriteLine(_describe(result));
        Leaderboard.ReportFindFirst(_label, stopwatch.Elapsed.TotalMilliseconds, result != null, stepsCount);
        return result;
    }

    public List<TElement> FindAll(TElement root)
    {
        var founds = new List<TElement>();
        var stepsCount = 0;
        var stopwatch = Stopwatch.StartNew();

        void Search(TElement node)
        {
            if (_matches(node)) founds.Add(node);

            var child = _getFirstChild(node);
            stepsCount++;
            while (child != null)
            {
                Search(child);

                child = _getNextSibling(child);
                stepsCount++;
            }
        }

        Search(root);
        stopwatch.Stop();
        Console.WriteLine($"[FindAll] time={stopwatch.Elapsed.TotalMilliseconds:F2}ms count={founds.Count} steps={stepsCount}");
        foreach (var (element, index) in founds.Select((element, index) => (element, index)))
            Console.WriteLine($"[{index}] - {_describe(element)}");
        Leaderboard.ReportFindAll(_label, stopwatch.Elapsed.TotalMilliseconds, founds.Count, stepsCount);
        return founds;
    }
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

public sealed class NativePropertyMatcher
{
    private readonly ConditionBase _condition;

    public NativePropertyMatcher(ConditionBase condition)
    {
        _condition = condition;
    }

    public bool Matches(Interop.UIAutomationClient.IUIAutomationElement element) => Matches(element, _condition);

    private bool Matches(Interop.UIAutomationClient.IUIAutomationElement element, ConditionBase condition) => condition switch
    {
        PropertyCondition propertyCondition => PropertyMatches(element, propertyCondition),
        AndCondition andCondition => andCondition.Conditions.All(child => Matches(element, child)),
        OrCondition orCondition => orCondition.Conditions.Any(child => Matches(element, child)),
        NotCondition notCondition => !Matches(element, notCondition.Condition),
        TrueCondition => true,
        FalseCondition => false,
        _ => throw new NotImplementedException($"Condition type {condition.GetType().Name} is not supported."),
    };

    private bool PropertyMatches(Interop.UIAutomationClient.IUIAutomationElement element, PropertyCondition propertyCondition)
    {
        var actual = Normalize(GetPropertyValue(element, propertyCondition.Property.Id));
        var expected = Normalize(propertyCondition.Value);
        return Equals(actual, expected);
    }

    private static object? Normalize(object? value) => value switch
    {
        ControlType controlType => ControlTypeConverter.ToControlTypeNative(controlType),
        Enum enumValue => Convert.ToInt32(enumValue),
        _ => value
    };

    private object? GetPropertyValue(Interop.UIAutomationClient.IUIAutomationElement element, int propertyId)
    {
        try { return element.GetCurrentPropertyValue(propertyId); }
        catch { return null; }
    }
}