using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUILibTest.Inspector;

namespace FlaUILibTest.UIDriver;

public class UILocator
{
    private readonly ModuleFinder _finder;
    private readonly ConditionBase _condition;

    internal UILocator(ModuleFinder finder, ConditionBase condition)
    {
        _finder = finder;
        _condition = condition;
    }

    public async Task ClickAsync() => await WithElement(el => el.Click());

    private async Task<AutomationElement> GetElement() => await _finder.RegisterAndGetElementAsync(_condition);
    private async Task<T> WithElement<T>(Func<AutomationElement, T> action) => action(await GetElement());
    private async Task WithElement(Action<AutomationElement> action) => action(await GetElement());
}
