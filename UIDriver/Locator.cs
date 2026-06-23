using FlaUI.Core.AutomationElements;

namespace UIDriver;

// Ленивый пользовательский хэндл. Несёт BY (собранный из условий пользователя) и при GetElementAsync
// сдаёт его AppManager'у. Что на том конце — не знает, просто связной.
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
