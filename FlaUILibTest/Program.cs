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
        for (var attempt = 0; attempt < 50 && window == null; attempt++)
        {
            window = root.FindFirst(TreeScope.TreeScope_Children, windowCondition);
            if (window == null) Thread.Sleep(200);
        }

        if (window == null)
        {
            Console.WriteLine("Excel window not found");
            return;
        }

        var propertyIds = new[]
        {
            UIA_NamePropertyId,
            UIA_ControlTypePropertyId,
            UIA_ClassNamePropertyId,
            UIA_AutomationIdPropertyId
        };

        var nativeCacheManager = new NativeCacheManager(automation);
        var trueCondition = automation.CreateTrueCondition();
        var result = nativeCacheManager.Find(window, trueCondition, propertyIds);

        PrintArray(result);
    }

    static void PrintArray(IUIAutomationElementArray array)
    {
        Console.WriteLine($"count = {array.Length}");
        for (var i = 0; i < array.Length; i++)
        {
            var element = array.GetElement(i);
            var name = element.GetCachedPropertyValue(UIA_NamePropertyId);
            var className = element.GetCachedPropertyValue(UIA_ClassNamePropertyId);
            var automationId = element.GetCachedPropertyValue(UIA_AutomationIdPropertyId);
            var controlType = element.GetCachedPropertyValue(UIA_ControlTypePropertyId);
            Console.WriteLine($"[{i}] controlType={controlType} name='{name}' class='{className}' automationId='{automationId}'");
        }
    }
}
