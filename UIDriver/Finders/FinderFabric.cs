namespace UIDriver;

public static class FinderFabric
{
    public static IFinder GetFinder(BY by) => new DescendantFinder(by.Element!);
}
