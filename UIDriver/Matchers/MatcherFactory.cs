namespace UIDriver.Matchers;

public static class MatcherFactory
{
    public static IMatcher GetMatcher(UIBy by)
    {
        IMatcher matcher;
        // conditions, trees, logic, decorators, etc. can be added here in the future
        matcher = new PropertyMatcher(by);

        return matcher;
    }
}
