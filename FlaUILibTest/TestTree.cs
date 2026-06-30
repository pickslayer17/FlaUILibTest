using FlaUI.Core.Definitions;

static class TestTree
{
    public static UiNode Build()
    {
        var desktop = Node(ControlType.Pane, name: "Desktop", className: "#32769");

        var taskbar = Node(ControlType.Pane, name: "Taskbar", className: "Shell_TrayWnd");
        var someOtherApp = Node(ControlType.Window, name: "Notepad", className: "Notepad");

        var excelMain = Node(ControlType.Window, name: "Book1 - Excel", className: "XLMAIN");

        var ribbon = Node(ControlType.Pane, name: "Ribbon", className: "NetUIHWND");
        var ribbonTabs = Node(ControlType.Tab, name: "Ribbon Tabs");
        var homeTab = Node(ControlType.TabItem, name: "Home");
        var insertTab = Node(ControlType.TabItem, name: "Insert");

        var workbookWindow = Node(ControlType.Pane, name: "Book1", className: "XLDESK");
        var sheetWindow = Node(ControlType.Window, name: "Book1", className: "EXCEL7");

        var sheetContentPane = Node(ControlType.Pane, automationId: "SheetContentPane");

        var cellA1 = Node(ControlType.DataItem, automationId: "A1", name: "A1");
        var cellB1 = Node(ControlType.DataItem, automationId: "B1", name: "B1");
        var cellC1 = Node(ControlType.DataItem, automationId: "C1", name: "C1");
        var cellA2 = Node(ControlType.DataItem, automationId: "A2", name: "A2");
        var cellB2 = Node(ControlType.DataItem, automationId: "B2", name: "B2");

        var statusBar = Node(ControlType.StatusBar, name: "Status Bar");

        Link(desktop, taskbar, excelMain, someOtherApp);
        Link(excelMain, ribbon, workbookWindow, statusBar);
        Link(ribbon, ribbonTabs);
        Link(ribbonTabs, homeTab, insertTab);
        Link(workbookWindow, sheetWindow);
        Link(sheetWindow, sheetContentPane);
        Link(sheetContentPane, cellA1, cellB1, cellC1, cellA2, cellB2);

        return desktop;
    }

    static UiNode Node(ControlType controlType, string name = null, string className = null, string automationId = null)
        => new UiNode
        {
            ControlType = controlType,
            Name = name,
            ClassName = className,
            AutomationId = automationId
        };

    static void Link(UiNode parent, params UiNode[] children)
    {
        parent.Children = children;
        foreach (var child in children)
            child.Parent = parent;
    }
}
