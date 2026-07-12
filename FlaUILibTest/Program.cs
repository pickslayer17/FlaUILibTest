using Interop.UIAutomationClient;
using System.Diagnostics;

class Program
{
    const int UIA_NamePropertyId = 30005;
    const int UIA_ControlTypePropertyId = 30003;
    const int UIA_ClassNamePropertyId = 30012;
    const int UIA_AutomationIdPropertyId = 30011;

    static void Main()
    {
        // CacheManager.RunStandalone();
        // return;

        var processStartInfo = new ProcessStartInfo(@"C:\Program Files\Microsoft Office\root\Office16\EXCEL.EXE", "/e")
        {
            WindowStyle = ProcessWindowStyle.Normal,
            UseShellExecute = false
        };
        Process.Start(processStartInfo);

        IUIAutomation automation = new CUIAutomation8();

        var root = automation.GetRootElement();
        var windowCondition = automation.CreatePropertyCondition(UIA_ClassNamePropertyId, "XLMAIN");

        IUIAutomationElement window = null;
        window = root.FindFirst(TreeScope.TreeScope_Children, windowCondition);

        var propertyIds = new[]
        {
            UIA_NamePropertyId,
            UIA_ControlTypePropertyId,
            UIA_ClassNamePropertyId,
            UIA_AutomationIdPropertyId
        };

        var nativeCacheManager = new NativeCacheManager(automation);
        var treeFilter = automation.CreatePropertyCondition(UIA_ControlTypePropertyId, 50029);
        var stopwatch = Stopwatch.StartNew();
        var result = nativeCacheManager.FindFirstBuildCache(window, windowCondition, propertyIds);
        stopwatch.Stop();
        Console.WriteLine($"Cached search time = {stopwatch.Elapsed.TotalMilliseconds:F2}ms");
        var cachedItemCount = result != null; //result.Length;//
        var cachedWindow = result;//result.GetElement(0);//

        var a1condition = automation.CreatePropertyCondition(UIA_AutomationIdPropertyId, "A1");
        // var u = 0;
        // while(u<5)
        // {
        //     u++;
        //     var a1Cell = cachedWindow.FindFirst(TreeScope.TreeScope_Descendants, a1condition);
        //     var clickable = a1Cell.GetClickablePoint(out tagPOINT tagPoint);
        //     Console.WriteLine($"{clickable} {tagPoint.x} {tagPoint.y}");
        // }

        // var structureHandler = new StructureChangedHandler();
        // automation.AddStructureChangedEventHandler(window, TreeScope.TreeScope_Subtree, null, structureHandler);
        // Console.WriteLine("subscribed to structure changed on window. tapem, potom Enter...");
        // Console.ReadLine();
        // automation.RemoveStructureChangedEventHandler(window, structureHandler);

        int count = 0;
        stopwatch = Stopwatch.StartNew();
        PrintTree(cachedWindow, 0, ref count);
        stopwatch.Stop();
        Console.WriteLine($"PrintTree time = {stopwatch.Elapsed.TotalMilliseconds:F2}ms");
        Console.WriteLine($"Elements count = {count}");
    }

    static void PrintTree(IUIAutomationElement element, int depth, ref int count)
    {
        count++;
        var name = element.GetCachedPropertyValue(UIA_NamePropertyId);
        var className = element.GetCachedPropertyValue(UIA_ClassNamePropertyId);
        var automationId = element.GetCachedPropertyValue(UIA_AutomationIdPropertyId);
        var controlType = element.GetCachedPropertyValue(UIA_ControlTypePropertyId);
        //Console.WriteLine($"{new string(' ', depth * 2)}controlType={controlType} name='{name}' class='{className}' automationId='{automationId}'");

        var children = element.GetCachedChildren();
        if (children == null) return;
        for (var i = 0; i < children.Length; i++)
            PrintTree(children.GetElement(i), depth + 1, ref count);
    }
}
