using FlaUI.Core.AutomationElements;

namespace UIDriver;

public sealed class Locator
{
    private readonly BY _by;
    private readonly ApplicationManager _app;

    public Locator(BY by, ApplicationManager app)
    {
        _by = by;
        _app = app;
    }

    public Task<AutomationElement> GetElementAsync() => _app.Submit(_by);
}
