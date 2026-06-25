namespace UIDriver;

public static class FinderFactory
{
    public static IFinder GetFinder(BY by) => new DescendantFinder(by);
}
