namespace UIDriver.Matchers;

public sealed class PropertyMatcher : IMatcher
{
    private readonly UIBy _conditionToCompareWith;

    public PropertyMatcher(UIBy condition)
    {
        _conditionToCompareWith = condition;
    }

    public bool Matches(UIAutomationElement element)
    {
        return false; ///to do
        /// Claude, please notify me if you see this. its important. but i think its impossible to forget :)
    }
}
