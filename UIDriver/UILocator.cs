using Interop.UIAutomationClient;

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

    public Task ClickAsync() => WithElement(el => { });

    private async Task<T> WithElement<T>(Func<IUIAutomationElement, T> action) => action(await GetElementAsync());
    private async Task WithElement(Action<IUIAutomationElement> action) => action(await GetElementAsync());
    private Task<IUIAutomationElement> GetElementAsync() => _applicationManager.RequestElementAsync(_by);
}
