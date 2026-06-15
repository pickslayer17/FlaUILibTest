using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using System.Diagnostics;

namespace FlaUILibTest.Inspector;

public class ModuleFinder
{
    private readonly List<(ConditionBase elementCondition, TaskCompletionSource<AutomationElement> tcs)> _watches = new();
    private readonly object _lock = new();
    private int _searchCount = 0;
    private Stopwatch _sw;

    public Task<AutomationElement> Register(ConditionBase elementCondition)
    {
        var tcs = new TaskCompletionSource<AutomationElement>(TaskCreationOptions.RunContinuationsAsynchronously);
        lock (_lock)
        {
            _watches.Add((elementCondition, tcs));
        }
        return tcs.Task;
    }

    public void Subscribe(Window window)
    {
        window.RegisterStructureChangedEvent(TreeScope.Subtree, OnStructureChanged);
    }
    
    private void OnStructureChanged(AutomationElement element, StructureChangeType changeType, int[] runtimeId)
    {
        if (changeType != StructureChangeType.ChildAdded) return;

        List<(ConditionBase elementCondition, TaskCompletionSource<AutomationElement> tcs)> snapshot;
        lock (_lock)
        {
            snapshot = new(_watches);
        }

        foreach (var watch in snapshot)
        {
            if (watch.tcs.Task.IsCompleted) continue;

            var searchNum = Interlocked.Increment(ref _searchCount);
            var cls = "";
            var name = "";
            try { cls = element.Properties.ClassName.ValueOrDefault; } catch { }
            try { name = element.Properties.Name.ValueOrDefault; } catch { }
            Console.WriteLine($"search from | {cls} | {name} |");
            if (cls == "MsoCommandBar" && _sw == null)
                _sw = Stopwatch.StartNew();
            try
            {
                var found = element.FindFirstDescendant(watch.elementCondition);
                if (found != null)
                {
                    Console.WriteLine($">>> FOUND after {searchNum} searches in {_sw.ElapsedMilliseconds}ms | module: {name} | {cls}");
                    watch.tcs.TrySetResult(found);
                }
            }
            catch { }
        }
    }
}