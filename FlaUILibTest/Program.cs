
using FlaUI.Core;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using FlaUILibTest;
using System.Diagnostics;

class Program
{
    static async Task Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var psi = new ProcessStartInfo(@"C:\Program Files\Microsoft Office\root\Office16\EXCEL.EXE", "/e")
        {
            WindowStyle = ProcessWindowStyle.Normal
        };
        var application = Application.Launch(psi);
        var automation = new UIA3Automation();
        var window = application.GetMainWindow(automation);
        var cf = automation.ConditionFactory;

        // 1. Подписка
        EventManager.Instance.Subscribe(window);

        // 2. Модуль — якорь XLDESK
        var gridModule = new Module(window, cf.ByClassName("XLDESK"));

        // 4. Элемент — ячейка A1
        var cellA1 = new Element(gridModule,
            cf.ByControlType(ControlType.DataItem).And(cf.ByName("A1")));

        await cellA1.ClickAsync();
    }
}
