using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUILibTest.Interfaces;

namespace FlaUILibTest.UIDriver;

public class UILocator
{
    private IElementSource _source;
    private readonly BY _condition;

    internal UILocator(IElementSource source, BY by)
    {
        _condition = by;
        _source = source;
    }

    public async Task ClickAsync() => await WithElement(el => el.Click());

    private async Task<AutomationElement> GetElement() => await _source.FindFirstAsync(_condition);
    private async Task<T> WithElement<T>(Func<AutomationElement, T> action) => action(await GetElement());
    private async Task WithElement(Action<AutomationElement> action) => action(await GetElement());
}
