using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.UIA3;
using FlaUILibTest.Extensions;
using System.Diagnostics;

public sealed class CustomSearch
{
    private readonly UIA3Automation _automation;

    public CustomSearch(UIA3Automation automation)
    {
        _automation = automation;
    }

    public List<AutomationElement> HybridSearchFind(AutomationElement root, ConditionBase condition, bool findAll = false)
    {
        var conditionWalker = _automation.TreeWalkerFactory.GetCustomTreeWalker(condition);
        var rawWalker = _automation.TreeWalkerFactory.GetRawViewWalker();
        var windowRuntimeId = Helpers.SafeRunTimeId(root);
        var desktopRuntimeId = Helpers.SafeRunTimeId(_automation.GetDesktop());
        var windowProcessId = Helpers.SafeProcessId(root);
        int stepsCount = 0;

        var stopwatch = Stopwatch.StartNew();

        var founds = new List<AutomationElement>();
        var node = conditionWalker.GetFirstChild(root);
        stepsCount++;

        if (findAll == false)
        {
            stopwatch.Stop();
            Console.WriteLine($"[FindFirst] time={stopwatch.Elapsed.TotalMilliseconds:F2}ms found={node != null} steps={stepsCount}");

            Leaderboard.ReportFindFirst("[12] FlaUI HybridTreeWalker", stopwatch.Elapsed.TotalMilliseconds, node != null, stepsCount);
            return new List<AutomationElement> { node };
        }

        while (node != null)
        {
            if (conditionWalker.GetFirstChild(node) != null) throw new Exception("oppa nihuya sebe!...");

            if (!IsPresentInWindow(rawWalker, node, windowProcessId, windowRuntimeId, desktopRuntimeId, ref stepsCount)) break;

            founds.Add(node);
            node = conditionWalker.GetNextSibling(node);
            stepsCount++;
        }

        stopwatch.Stop();

        Console.WriteLine($"[FindAll] time={stopwatch.Elapsed.TotalMilliseconds:F2}ms count={founds.Count} steps={stepsCount}");
        foreach (var (element, index) in founds.Select((element, index) => (element, index)))
            Console.WriteLine($"[{index}] - {Helpers.SafeName(element)} {Helpers.SafeRunTimeId(element).ToFormattedString()}");

        Leaderboard.ReportFindAll("[12] FlaUI HybridTreeWalker", stopwatch.Elapsed.TotalMilliseconds, founds.Count, stepsCount);

        return founds;
    }

    private bool IsPresentInWindow(ITreeWalker rawWalker, AutomationElement element, int windowProcessId, int[] windowRuntimeId, int[] desktopRuntimeId, ref int count)
    {
        var elementProcessId = Helpers.SafeProcessId(element);
        count++;
        if (elementProcessId == 0) throw new Exception("oppa nihuya sebe! element bez ProcessId");
        if (elementProcessId != windowProcessId) return false;

        var parent = rawWalker.GetParent(element);
        count++;
        while (parent != null)
        {
            var parentRuntimeId = Helpers.SafeRunTimeId(parent);
            if (parentRuntimeId.SequenceEqual(windowRuntimeId)) return true;
            if (parentRuntimeId.SequenceEqual(desktopRuntimeId)) return false;

            parent = rawWalker.GetParent(parent);
            count++;
        }

        throw new Exception("oppa nihuya sebe! desktop proeban, parent == null");
    }
}
