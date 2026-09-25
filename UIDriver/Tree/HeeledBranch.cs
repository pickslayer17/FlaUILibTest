namespace CacheManagement;

public sealed class HeeledBranch : Branch
{
    public UiNode Heel { get; }

    public HeeledBranch(UiNode tree, UiNode heel) : base(tree)
    {
        Heel = heel;
    }
}
