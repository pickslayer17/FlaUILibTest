using Interop.UIAutomationClient;
using UIDriver.CustomModels;

namespace CacheManagement;

public class UiNode
{
    public UiNode Parent { get; set; }
    public UiNode[] Children { get; set; }
    public IUIAutomationElement Element { get; init; }
    public CachedRunTimeId RunTimeId { get; set; }

    public int ControlType;
    public string Name;

    public UiNode(IUIAutomationElement element)
    {
        Element = element;
    }

    public IEnumerable<UiNode> Traverse()
    {
        yield return this;

        foreach (var child in Children ?? [])
            foreach (var descendant in child.Traverse())
                yield return descendant;
    }
}
