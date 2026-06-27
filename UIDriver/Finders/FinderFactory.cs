namespace UIDriver;

public static class FinderFactory
{
    public static IFinder GetFinder(BY by)
    {
        IFinder finder;
        // conditions, trees, logic, decorators, etc. can be added here in the future
        finder = new DescendantFinder(by);

        return finder;
    }
}
