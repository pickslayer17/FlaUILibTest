using FlaUI.Core.AutomationElements;

namespace UIDriver.Matchers;

public interface IMatcher
{
    bool Matches(AutomationElementObject element);
}
