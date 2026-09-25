using Interop.UIAutomationClient;
using UIDriver.Uia.Constants;

namespace UIDriver.Uia;

public sealed class CacheProfile
{
    private static readonly UiaProperty[] TreeProperties =
    [
        UiaProperty.RuntimeId,
        UiaProperty.ControlType,
        UiaProperty.Name
    ];

    public static CacheProfile Subtree { get; } = new(TreeProperties, TreeScope.TreeScope_Subtree);
    public static CacheProfile SingleElement { get; } = new(TreeProperties, TreeScope.TreeScope_Element);

    private readonly TreeScope _treeScope;

    public IReadOnlyList<UiaProperty> Properties { get; }

    private CacheProfile(IReadOnlyList<UiaProperty> properties, TreeScope treeScope)
    {
        Properties = properties;
        _treeScope = treeScope;
    }

    internal IUIAutomationCacheRequest CreateRequest(UiaAutomation automation)
    {
        var cacheRequest = automation.Native.CreateCacheRequest();
        cacheRequest.TreeScope = _treeScope;
        cacheRequest.AutomationElementMode = AutomationElementMode.AutomationElementMode_Full;
        foreach (var property in Properties)
            cacheRequest.AddProperty((int)property);

        return cacheRequest;
    }
}
