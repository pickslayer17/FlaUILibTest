using UIDriver.Uia;
using UIDriver.Windows;

namespace UIDriver.Api;

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

    private async Task<T> WithElement<T>(Func<UiaElement, T> action) => action(await GetElementAsync());
    private async Task WithElement(Action<UiaElement> action) => action(await GetElementAsync());
    private Task<UiaElement> GetElementAsync() => _applicationManager.RequestElementAsync(_by);
}
