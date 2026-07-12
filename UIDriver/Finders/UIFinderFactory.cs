using UIDriver.Finders.Finders;

namespace UIDriver;

public static class UIFinderFactory
{
    public static IFinder GetFinder(UIBy by)
    {
        IFinder finder;
        finder = new SelfFinder(by);

        return finder;
    }
}
