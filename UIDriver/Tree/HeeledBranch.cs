namespace UIDriver.Tree;

public sealed class HeeledBranch : Branch
{
    public UiNode Heel { get; }

    public override UiNode Top => Heel;

    public HeeledBranch(UiNode tree, UiNode heel) : base(tree)
    {
        Heel = heel;
    }
}
