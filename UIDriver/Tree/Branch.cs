namespace UIDriver.Tree;

public class Branch
{
    public UiNode Tree { get; }

    public virtual UiNode Top => Tree;

    public Branch(UiNode tree)
    {
        Tree = tree;
    }

    public bool AllDescendantsHaveRunTimeId() => Descendants().All(node => node.IsRunTimeIdExists);

    public bool NoDescendantHasRunTimeId() => !Descendants().Any(node => node.IsRunTimeIdExists);

    private IEnumerable<UiNode> Descendants() => Tree.Traverse().Skip(1);
}
