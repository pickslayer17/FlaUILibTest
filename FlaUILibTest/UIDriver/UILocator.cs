using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUILibTest.Interfaces;

namespace FlaUILibTest.UIDriver;

public class UILocator
{
    private IElementSource _source;
    public readonly BY Condition;

    internal UILocator(IElementSource source, BY by)
    {
        Condition = by;
        _source = source;
    }

    public async Task<string> GetTextAsync() => await WithElement(el => el.AsTextBox().Text);
    public async Task SetTextAsync(string value) => await WithElement(el => el.AsTextBox().Text = value);
    public async Task WaitAsync() => await WithElement(el => el);
    public async Task InvokeAsync() => await WithElement(el => el.Patterns.Invoke.Pattern.Invoke());

    public async Task ClickAsync() => await WithElement(el => el.Click());

    private async Task<AutomationElement> GetElement() => await _source.FindFirstAsync(Condition);
    private async Task<T> WithElement<T>(Func<AutomationElement, T> action) => action(await GetElement());
    private async Task WithElement(Action<AutomationElement> action) => action(await GetElement());
}
