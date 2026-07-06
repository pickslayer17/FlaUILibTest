using FlaUI.Core.Definitions;

static class TestTree
{
    public static UiNode Build()
    {
        var desktop = Node(ControlType.Pane, name: "Desktop", className: "#32769");

        var taskbar = Node(ControlType.Pane, name: "Taskbar", className: "Shell_TrayWnd");
        var notepad = Node(ControlType.Window, name: "Notepad", className: "Notepad");
        var chrome = Node(ControlType.Window, name: "Google Chrome", className: "Chrome_WidgetWin_1");
        var calculator = Node(ControlType.Window, name: "Calculator", className: "ApplicationFrameWindow");

        var excelMain = Node(ControlType.Window, name: "Book1 - Excel", className: "XLMAIN");

        var ribbon = Node(ControlType.Pane, name: "Ribbon", className: "NetUIHWND");
        var ribbonTabs = Node(ControlType.Tab, name: "Ribbon Tabs");
        var fileTab = Node(ControlType.TabItem, name: "File");
        var homeTab = Node(ControlType.TabItem, name: "Home");
        var insertTab = Node(ControlType.TabItem, name: "Insert");
        var pageLayoutTab = Node(ControlType.TabItem, name: "Page Layout");
        var formulasTab = Node(ControlType.TabItem, name: "Formulas");
        var dataTab = Node(ControlType.TabItem, name: "Data");
        var reviewTab = Node(ControlType.TabItem, name: "Review");
        var viewTab = Node(ControlType.TabItem, name: "View");

        var quickAccessToolbar = Node(ControlType.ToolBar, name: "Quick Access Toolbar");
        var saveButton = Node(ControlType.Button, name: "Save", automationId: "QAT_Save");
        var undoButton = Node(ControlType.Button, name: "Undo", automationId: "QAT_Undo");
        var redoButton = Node(ControlType.Button, name: "Redo", automationId: "QAT_Redo");

        var formulaBar = Node(ControlType.Pane, name: "Formula Bar", className: "NetUIHWND");
        var nameBox = Node(ControlType.Edit, name: "Name Box", automationId: "NameBox");
        var formulaEdit = Node(ControlType.Edit, name: "Formula", automationId: "FormulaEdit");

        var workbookWindow = Node(ControlType.Pane, name: "Book1", className: "XLDESK");
        var sheetWindow = Node(ControlType.Window, name: "Book1", className: "EXCEL7");

        var decoyContentPane = Node(ControlType.Pane, name: "Decoy", automationId: "SheetContentPane");
        var decoyCellA1 = Node(ControlType.DataItem, automationId: "A1", name: "A1");
        var decoyCellB1 = Node(ControlType.DataItem, automationId: "C1", name: "C1");

        var sheetContentPane = Node(ControlType.Pane, automationId: "SheetContentPane");

        var rowHeaderPane = Node(ControlType.Pane, name: "Row Headers", automationId: "RowHeaders");
        var rowHeader1 = Node(ControlType.HeaderItem, name: "1", automationId: "R1");
        var rowHeader2 = Node(ControlType.HeaderItem, name: "2", automationId: "R2");
        var rowHeader3 = Node(ControlType.HeaderItem, name: "3", automationId: "R3");

        var columnHeaderPane = Node(ControlType.Pane, name: "Column Headers", automationId: "ColumnHeaders");
        var columnHeaderA = Node(ControlType.HeaderItem, name: "A", automationId: "CA");
        var columnHeaderB = Node(ControlType.HeaderItem, name: "B", automationId: "CB");
        var columnHeaderC = Node(ControlType.HeaderItem, name: "C", automationId: "CC");

        var gridPane = Node(ControlType.Pane, name: "Grid", automationId: "GridPane");

        var decoyA1 = Node(ControlType.DataItem, automationId: "A1", name: "A1");
        var decoyA2 = Node(ControlType.DataItem, automationId: "A2", name: "A2");
        var decoyA3 = Node(ControlType.DataItem, automationId: "A3", name: "A3");
        var decoyB2 = Node(ControlType.DataItem, automationId: "B1", name: "B1", className: "real target");
        var decoyC3 = Node(ControlType.DataItem, automationId: "C1", name: "C1");

        var targetA1 = Node(ControlType.DataItem, automationId: "A1", name: "A1");
        var targetB1 = Node(ControlType.DataItem, automationId: "B1", name: "B1", className: "false target");
        var targetC1 = Node(ControlType.DataItem, automationId: "C1", name: "C1");
        var targetD1 = Node(ControlType.DataItem, automationId: "D1", name: "D1");

        var targetA2 = Node(ControlType.DataItem, automationId: "A2", name: "A2");
        var targetB2 = Node(ControlType.DataItem, automationId: "B2", name: "B2");
        var targetC2 = Node(ControlType.DataItem, automationId: "C2", name: "C2");

        var targetA3 = Node(ControlType.DataItem, automationId: "A3", name: "A3");
        var targetB3 = Node(ControlType.DataItem, automationId: "B3", name: "B3");

        var scrollBarVertical = Node(ControlType.ScrollBar, name: "Vertical", className: "NetUIScrollBar");
        var scrollBarHorizontal = Node(ControlType.ScrollBar, name: "Horizontal", className: "NetUIScrollBar");

        var sheetTabPane = Node(ControlType.Pane, name: "Sheet Tabs", automationId: "SheetTabs");
        var sheet1Tab = Node(ControlType.TabItem, name: "Sheet1", automationId: "Tab1");
        var sheet2Tab = Node(ControlType.TabItem, name: "Sheet2", automationId: "Tab2");
        var sheet3Tab = Node(ControlType.TabItem, name: "Sheet3", automationId: "Tab3");

        var statusBar = Node(ControlType.StatusBar, name: "Status Bar");
        var readyText = Node(ControlType.Text, name: "Ready", automationId: "StatusReady");
        var zoomSlider = Node(ControlType.Slider, name: "Zoom", automationId: "ZoomSlider");

        Link(desktop, taskbar, notepad, chrome, excelMain, calculator);

        Link(excelMain, ribbon, quickAccessToolbar, formulaBar, workbookWindow, statusBar);

        Link(ribbon, ribbonTabs);
        Link(ribbonTabs, fileTab, homeTab, insertTab, pageLayoutTab, formulasTab, dataTab, reviewTab, viewTab);

        Link(quickAccessToolbar, saveButton, undoButton, redoButton);

        Link(formulaBar, nameBox, formulaEdit);

        Link(workbookWindow, decoyContentPane, sheetWindow);
        Link(decoyContentPane, decoyCellA1, decoyCellB1);

        Link(sheetWindow, sheetContentPane, scrollBarVertical, scrollBarHorizontal, sheetTabPane);

        Link(sheetContentPane, rowHeaderPane, columnHeaderPane, gridPane);
        Link(rowHeaderPane, rowHeader1, rowHeader2, rowHeader3);
        Link(columnHeaderPane, columnHeaderA, columnHeaderB, columnHeaderC);

        Link(gridPane,
            decoyA1, decoyA2, decoyA3, decoyB2, decoyC3,
            targetA1, targetB1, targetC1, targetD1,
            targetA2, targetB2, targetC2,
            targetA3, targetB3);

        Link(sheetTabPane, sheet1Tab, sheet2Tab, sheet3Tab);

        Link(statusBar, readyText, zoomSlider);

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
