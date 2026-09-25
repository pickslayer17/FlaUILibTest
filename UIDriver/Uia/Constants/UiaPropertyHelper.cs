namespace UIDriver.Uia.Constants;

public static class UiaPropertyHelper
{
    public static readonly int[] AllProperties = Enum.GetValues<UiaProperty>().Select(property => (int)property).ToArray();
    public static int GetPropertyId(UiaProperty property) => (int)property;
}
