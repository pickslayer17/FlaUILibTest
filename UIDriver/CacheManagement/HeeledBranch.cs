public sealed class HeeledBranch : Branch
{
    public UiNode Heel { get; }

    public HeeledBranch(UiNode heel, UiNode tree) : base(tree)
    {
        Heel = heel;
    }
}
