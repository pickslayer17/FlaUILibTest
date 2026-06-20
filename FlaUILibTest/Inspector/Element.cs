using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;

namespace FlaUILibTest.Inspector;

public class Element : AutomationSubscriberBase
{
    public Element(WindowFinder moduleFinder, ConditionBase condition) : base(moduleFinder, condition)
    {
    }

    public Task<string> GetNameAsync() => WithElement(el => el.Properties.Name.ValueOrDefault ?? "");
    public Task<string> GetValueAsync() => WithElement(el => el.Patterns.Value.Pattern.Value?.ToString());
    public Task ClickAsync() => WithElement(el => el.Click());

    private async Task<T> WithElement<T>(Func<AutomationElement, T> action) => action(await GetElement());
    private async Task WithElement(Action<AutomationElement> action) => action(await GetElement());
}