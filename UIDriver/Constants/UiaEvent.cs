namespace UIDriver;

// Values mirror UIAutomationClient.h from Windows SDK 10.0.26100.0.
public enum UiaEvent
{
    ToolTipOpened = 20000, ToolTipClosed = 20001, StructureChanged = 20002,
    MenuOpened = 20003, AutomationPropertyChanged = 20004, AutomationFocusChanged = 20005,
    AsyncContentLoaded = 20006, MenuClosed = 20007, LayoutInvalidated = 20008,
    InvokeInvoked = 20009, SelectionItemElementAddedToSelection = 20010,
    SelectionItemElementRemovedFromSelection = 20011, SelectionItemElementSelected = 20012,
    SelectionInvalidated = 20013, TextTextSelectionChanged = 20014,
    TextTextChanged = 20015, WindowOpened = 20016, WindowClosed = 20017,
    MenuModeStart = 20018, MenuModeEnd = 20019, InputReachedTarget = 20020,
    InputReachedOtherElement = 20021, InputDiscarded = 20022, SystemAlert = 20023,
    LiveRegionChanged = 20024, HostedFragmentRootsInvalidated = 20025,
    DragDragStart = 20026, DragDragCancel = 20027, DragDragComplete = 20028,
    DropTargetDragEnter = 20029, DropTargetDragLeave = 20030,
    DropTargetDropped = 20031, TextEditTextChanged = 20032,
    TextEditConversionTargetChanged = 20033, Changes = 20034, Notification = 20035,
    ActiveTextPositionChanged = 20036
}
