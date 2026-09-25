using UIDriver.Uia;
using UIDriver.Uia.Constants;

namespace UIDriver.Tree;

public class UiNode
{
    public UiNode? Parent { get; set; }
    public UiNode[] Children { get; set; } = [];
    public UiaElement Element { get; }
    public RunTimeId? RunTimeId { get; }
    public bool IsRunTimeIdExists => RunTimeId is not null;
    public UiaControlType ControlType { get; }
    public string? Name { get; }
    public bool IsDirty { get; set; }

    public UiNode(UiaElement element, RunTimeId? runTimeId, UiaControlType controlType, string? name)
    {
        Element = element;
        RunTimeId = runTimeId;
        ControlType = controlType;
        Name = name;
    }

    public IEnumerable<UiNode> Traverse()
    {
        yield return this;

        foreach (var child in Children)
            foreach (var descendant in child.Traverse())
                yield return descendant;
    }
}
