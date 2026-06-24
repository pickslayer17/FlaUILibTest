using FlaUI.Core.AutomationElements;

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

    public Task ClickAsync() => WithElement(el => el.Element.Click());

    private async Task<T> WithElement<T>(Func<AutomationElementObject, T> action) => action(await GetElementAsync());
    private async Task WithElement(Action<AutomationElementObject> action) => action(await GetElementAsync());
    private Task<AutomationElementObject> GetElementAsync() => _applicationManager.RequestElementAsync(_by);
}
