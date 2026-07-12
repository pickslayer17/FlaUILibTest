namespace UIDriver;

public enum UiaProperty
{
    RuntimeId = 30000,
    BoundingRectangle = 30001,
    ProcessId = 30002,
    ControlType = 30003,
    LocalizedControlType = 30004,
    Name = 30005,
    AcceleratorKey = 30006,
    AccessKey = 30007,
    HasKeyboardFocus = 30008,
    IsKeyboardFocusable = 30009,
    IsEnabled = 30010,
    AutomationId = 30011,
    ClassName = 30012,
    HelpText = 30013,
    ClickablePoint = 30014,
    Culture = 30015,
    IsControlElement = 30016,
    IsContentElement = 30017,
    LabeledBy = 30018,
    IsPassword = 30019,
    NativeWindowHandle = 30020,
    ItemType = 30021,
    IsOffscreen = 30022,
    Orientation = 30023,
    FrameworkId = 30024,
    IsRequiredForForm = 30025,
    ItemStatus = 30026,
    AriaRole = 30101,
    AriaProperties = 30102,
    IsDataValidForForm = 30103,
    ControllerFor = 30104,
    DescribedBy = 30105,
    FlowsTo = 30106,
    ProviderDescription = 30107,
    OptimizeForVisualContent = 30111,
    LiveSetting = 30135,
    FlowsFrom = 30148,
    IsPeripheral = 30150,
    PositionInSet = 30152,
    SizeOfSet = 30153,
    Level = 30154,
    AnnotationTypes = 30155,
    AnnotationObjects = 30156,
    LandmarkType = 30157,
    LocalizedLandmarkType = 30158,
    FullDescription = 30159,
    FillColor = 30160,
    OutlineColor = 30161,
    FillType = 30162,
    VisualEffects = 30163,
    OutlineThickness = 30164,
    CenterPoint = 30165,
    Rotation = 30166,
    Size = 30167,
    HeadingLevel = 30173,
    IsDialog = 30174
}

public static class UiaPropertyHelper
{
    public static readonly int[] AllProperties = Enum.GetValues<UiaProperty>()
        .Select(property => (int)property)
        .ToArray();

    public static int GetPropertyId(UiaProperty property) => (int)property;
}
