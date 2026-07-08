using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.UIA3;
using FlaUI.UIA3.Converters;

public static class TreeSearchExperiments
{
    public static void Run(UIA3Automation automation, AutomationElement mainWindow, ConditionBase targetCondition)
    {
        var nativeAutomation = automation.NativeAutomation;
        var nativeRoot = ((UIA3FrameworkAutomationElement)mainWindow.FrameworkAutomationElement).NativeElement;

        var cacheManager = new CacheManager(automation);
        var customSearch = new CustomSearch(automation);

        Console.WriteLine("\n=== CACHED SEARCH ===");
        cacheManager.CachedSearch(mainWindow, targetCondition, out var mainWindowCached);
        var uiNodeTree = Helpers.BuildUINodeTree(mainWindowCached, null);

        Console.WriteLine("\n=== HYBRID SEARCH ===");
        customSearch.HybridSearchFind(mainWindow, targetCondition);
        customSearch.HybridSearchFind(mainWindow, targetCondition, true);

        var convertedCondition = ConditionConverter.ToNative(automation, targetCondition);
        var nativeMatcher = new NativePropertyMatcher(targetCondition).Matches;
        Func<Interop.UIAutomationClient.IUIAutomationElement, object?> describeNative = element =>
        {
            string name = "";
            bool? isEnabled = null;
            try { name = element.CurrentName; } catch { name = "not supported"; };
            try { isEnabled = element.CurrentIsEnabled != 0; } catch { }
            var enabled = isEnabled.HasValue ? isEnabled.Value ? "true" : "false" : "not supported";

            return $"name = '{name}'. isEnabled = '{enabled}'";
        };

        TreeSearch<Interop.UIAutomationClient.IUIAutomationTreeWalker, Interop.UIAutomationClient.IUIAutomationElement> NativeSearch(string label, Interop.UIAutomationClient.IUIAutomationTreeWalker walker)
            => new(label, walker.GetFirstChildElement, walker.GetNextSiblingElement, nativeMatcher, describeNative);

        var flaUiMatcher = new PropertyMatcher(targetCondition).Matches;
        Func<AutomationElement, object?> describeFlaUi = element => { try { return element.Name; } catch { return null; } };

        TreeSearch<ITreeWalker, AutomationElement> FlaUiSearch(string label, ITreeWalker walker)
            => new(label, walker.GetFirstChild, walker.GetNextSibling, flaUiMatcher, describeFlaUi);

        Console.WriteLine("\n=== [1] native CreateTreeWalker(RawViewCondition) ===");
        NativeSearch("[1] native Create+RawViewCondition", nativeAutomation.CreateTreeWalker(nativeAutomation.RawViewCondition)).FindFirst(nativeRoot);
        NativeSearch("[1] native Create+RawViewCondition", nativeAutomation.CreateTreeWalker(nativeAutomation.RawViewCondition)).FindAll(nativeRoot);

        Console.WriteLine("\n=== [2] native CreateTreeWalker(ControlViewCondition) ===");
        NativeSearch("[2] native Create+ControlViewCondition", nativeAutomation.CreateTreeWalker(nativeAutomation.ControlViewCondition)).FindFirst(nativeRoot);
        NativeSearch("[2] native Create+ControlViewCondition", nativeAutomation.CreateTreeWalker(nativeAutomation.ControlViewCondition)).FindAll(nativeRoot);

        Console.WriteLine("\n=== [3] native CreateTreeWalker(ContentViewCondition) ===");
        NativeSearch("[3] native Create+ContentViewCondition", nativeAutomation.CreateTreeWalker(nativeAutomation.ContentViewCondition)).FindFirst(nativeRoot);
        NativeSearch("[3] native Create+ContentViewCondition", nativeAutomation.CreateTreeWalker(nativeAutomation.ContentViewCondition)).FindAll(nativeRoot);

        Console.WriteLine("\n=== [4] native CreateTreeWalker(convertedCondition) ===");
        NativeSearch("[4] native Create+convertedCondition", nativeAutomation.CreateTreeWalker(convertedCondition)).FindFirst(nativeRoot);
        NativeSearch("[4] native Create+convertedCondition", nativeAutomation.CreateTreeWalker(convertedCondition)).FindAll(nativeRoot);

        Console.WriteLine("\n=== [5] native RawViewWalker ===");
        NativeSearch("[5] native RawViewWalker", nativeAutomation.RawViewWalker).FindFirst(nativeRoot);
        NativeSearch("[5] native RawViewWalker", nativeAutomation.RawViewWalker).FindAll(nativeRoot);

        Console.WriteLine("\n=== [6] native ControlViewWalker ===");
        NativeSearch("[6] native ControlViewWalker", nativeAutomation.ControlViewWalker).FindFirst(nativeRoot);
        NativeSearch("[6] native ControlViewWalker", nativeAutomation.ControlViewWalker).FindAll(nativeRoot);

        Console.WriteLine("\n=== [7] native ContentViewWalker ===");
        NativeSearch("[7] native ContentViewWalker", nativeAutomation.ContentViewWalker).FindFirst(nativeRoot);
        NativeSearch("[7] native ContentViewWalker", nativeAutomation.ContentViewWalker).FindAll(nativeRoot);

        Console.WriteLine("\n=== [8] FlaUI RawViewWalker ===");
        FlaUiSearch("[8] FlaUI RawViewWalker", automation.TreeWalkerFactory.GetRawViewWalker()).FindFirst(mainWindow);
        FlaUiSearch("[8] FlaUI RawViewWalker", automation.TreeWalkerFactory.GetRawViewWalker()).FindAll(mainWindow);

        Console.WriteLine("\n=== [9] FlaUI ControlViewWalker ===");
        FlaUiSearch("[9] FlaUI ControlViewWalker", automation.TreeWalkerFactory.GetControlViewWalker()).FindFirst(mainWindow);
        FlaUiSearch("[9] FlaUI ControlViewWalker", automation.TreeWalkerFactory.GetControlViewWalker()).FindAll(mainWindow);

        Console.WriteLine("\n=== [10] FlaUI ContentViewWalker ===");
        FlaUiSearch("[10] FlaUI ContentViewWalker", automation.TreeWalkerFactory.GetContentViewWalker()).FindFirst(mainWindow);
        FlaUiSearch("[10] FlaUI ContentViewWalker", automation.TreeWalkerFactory.GetContentViewWalker()).FindAll(mainWindow);

        Console.WriteLine("\n=== [11] FlaUI GetCustomTreeWalker(targetCondition) ===");
        FlaUiSearch("[11] FlaUI GetCustomTreeWalker", automation.TreeWalkerFactory.GetCustomTreeWalker(targetCondition)).FindFirst(mainWindow);
        FlaUiSearch("[11] FlaUI GetCustomTreeWalker", automation.TreeWalkerFactory.GetCustomTreeWalker(targetCondition)).FindAll(mainWindow);

        Leaderboard.PrintResults();
    }
}
