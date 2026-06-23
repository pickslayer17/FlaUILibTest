namespace UIDriver;

public sealed class FinderFabric
{
    public IFinder GetFinder(BY by)
    {
        IFinder strategy = by.Parent is not null
            ? new ParentFinder(by.Parent, by.Element!)
            : new DescendantFinder(by.Element!);

        return strategy;
    }
}
