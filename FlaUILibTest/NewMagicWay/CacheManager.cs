using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using System.Diagnostics;

public sealed class CacheManager
{
    private readonly UIA3Automation _automation;
    private double _cacheWholeSearchTime;

    public CacheManager(UIA3Automation automation)
    {
        _automation = automation;
    }

    public List<AutomationElement> CachedSearch(AutomationElement root, ConditionBase condition, out AutomationElement mainWindowCachedOut)
    {
        AutomationElement mainWindowCached = null;
        var mainWindowCondition = _automation.ConditionFactory
        .ByControlType(ControlType.Window)
        .And(_automation.ConditionFactory
            .ByName("Book1 - Excel")
            .Or(_automation.ConditionFactory
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
        }, message: "find directly with filter");

        var result = new List<AutomationElement>();

        var cacheWatch = Stopwatch.StartNew();
        FindInCache(mainWindowCached, condition, result);
        cacheWatch.Stop();
        _cacheWholeSearchTime += cacheWatch.Elapsed.TotalMilliseconds;
        Console.WriteLine($"[ebaaaaat v nachale {cacheWatch.Elapsed.TotalMilliseconds:F2}ms");

        ExecuteInCacheMode(() =>
        {
             FindInCache(mainWindowCached, condition, result);
        }, message: "Find in cache");

        cacheWatch = Stopwatch.StartNew();
        FindInCache(mainWindowCached, condition, result);
        cacheWatch.Stop();
        _cacheWholeSearchTime += cacheWatch.Elapsed.TotalMilliseconds;
        Console.WriteLine($"[ebaaaaat {cacheWatch.Elapsed.TotalMilliseconds:F2}ms");
        Console.WriteLine($"Whole time in for cache actions: {_cacheWholeSearchTime:f2}ms");
        Leaderboard.ReportFindAll("[13] CachedSearch", _cacheWholeSearchTime, result.Count, 0);

        mainWindowCachedOut = mainWindowCached;
        return result;
    }

    public void FindInCache(AutomationElement cache, ConditionBase condition, List<AutomationElement> result)
    {
        var native = ((UIA3FrameworkAutomationElement)cache.FrameworkAutomationElement).NativeElement;
        if (new CachedNativePropertyMatcher(condition).Matches(native)) result.Add(cache);

        foreach (var child in cache.CachedChildren)
        {
            FindInCache(child, condition, result);
        }
    }

    public CacheRequest ExecuteInCacheMode(Action action, ConditionBase treeFilter = null, string message = null)
    {
        CacheRequest cacheRequest;
        var cacheWatch = Stopwatch.StartNew();

        cacheRequest = new CacheRequest
        {
            TreeScope = TreeScope.Subtree,
            AutomationElementMode = AutomationElementMode.Full
        };

        if (treeFilter != null)
            cacheRequest.TreeFilter = treeFilter;

        cacheRequest.Add(_automation.PropertyLibrary.Element.ProcessId);
        cacheRequest.Add(_automation.PropertyLibrary.Element.RuntimeId);
        cacheRequest.Add(_automation.PropertyLibrary.Element.Name);
        cacheRequest.Add(_automation.PropertyLibrary.Element.ControlType);
        cacheRequest.Add(_automation.PropertyLibrary.Element.AutomationId);
        cacheRequest.Add(_automation.PropertyLibrary.Element.ClassName);
        using (cacheRequest.Activate())
        {
            action.Invoke();
        }

        cacheWatch.Stop();
        _cacheWholeSearchTime += cacheWatch.Elapsed.TotalMilliseconds;
        Console.WriteLine($"[ExecuteInCacheMode] '{message}' completed. Time: {cacheWatch.Elapsed.TotalMilliseconds:F2}ms");
        return cacheRequest;
    }
}
