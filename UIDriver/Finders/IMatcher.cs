using FlaUI.Core.AutomationElements;

namespace UIDriver;

public interface IMatcher
{
    bool Matches(AutomationElement element, BY condition);
}
