using FlaUI.Core.AutomationElements;

namespace UIDriver.Matchers;

public interface IMatcher
{
    bool Matches(UIAutomationElement element);
}
