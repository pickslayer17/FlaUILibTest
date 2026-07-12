using FlaUI.Core.AutomationElements;

namespace UIDriver;

public sealed class UILocator
{
    private readonly UIBy _by;
    private readonly UIApplicationManager _applicationManager;

    internal UILocator(UIBy by, UIApplicationManager applicationManager)
    {
        _by = by;
        _applicationManager = applicationManager;
    }

    public Task ClickAsync() => WithElement(el => el.Element.Click());

    private async Task<T> WithElement<T>(Func<UIAutomationElement, T> action) => action(await GetElementAsync());
    private async Task WithElement(Action<UIAutomationElement> action) => action(await GetElementAsync());
    private Task<UIAutomationElement> GetElementAsync() => _applicationManager.RequestElementAsync(_by);
}
