using UIDriver.Finders.Finders;

namespace UIDriver;

public static class FinderFactory
{
    public static IFinder GetFinder(BY by)
    {
        IFinder finder;
        finder = new SelfFinder(by);


        // this logic not implemented at all. and that the most interesting part - we will talk about it later
        if (by.Parent != null)
        {
            
        }

        if (by.Ancestor != null)
        {
            new AncestorFinder(finder);
        }
        if (by.Children != null)
        {

        }
        if (by.Descendants != null)
        {

        }
        if (by.FollowingSiblings != null)
        {

        }
        if (by.Following != null)
        {

        }
        if (by.Previous != null)
        {

        }
        if (by.PreviousSiblings != null)
        {

        }





        return finder;
    }
}
