namespace UIDriver;

public sealed class Locator
{
    private readonly BY _by;
    private readonly ApplicationManager _applicationManager;

    internal Locator(BY by, ApplicationManager applicationManager)
    {
        _by = by;
        _applicationManager = applicationManager;
    }

    public Task<AutomationElementObject> GetElementAsync() => _applicationManager.Request(_by);
}
