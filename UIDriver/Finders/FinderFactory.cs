using UIDriver.Finders;
using UIDriver.Finders.Decorators;
using UIDriver.Finders.Finders;

namespace UIDriver;

public static class FinderFactory
{
    public static IFinder GetFinder(BY by)
    {
        IFinder finder;
        finder = new SelfFinder(by);

        if (by.Parent != null)
        {
            new AncestorDecorator(finder);
        }

        if (by.Ancestor != null)
        {

        }
        if (by.Children != null)
        {

        }
        if (by.Descendants != null)
        {

        }
        if (by.Siblings != null)
        {

        }



        

        return finder;
    }
}
