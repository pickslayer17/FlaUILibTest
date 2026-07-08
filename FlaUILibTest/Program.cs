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
        var result = nativeCacheManager.Find(root, windowCondition, propertyIds);

        var cachedWindow = result.GetElement(0);

        int count = 0;
        var stopwatch = Stopwatch.StartNew();
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
        Console.WriteLine($"{new string(' ', depth * 2)}controlType={controlType} name='{name}' class='{className}' automationId='{automationId}'");

        var children = element.GetCachedChildren();
        if (children == null) return;
        for (var i = 0; i < children.Length; i++)
            PrintTree(children.GetElement(i), depth + 1, ref count);
    }
}
