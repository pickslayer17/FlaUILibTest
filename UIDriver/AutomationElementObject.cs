using FlaUI.Core.AutomationElements;

namespace UIDriver;

public sealed class AutomationElementObject
{
    public AutomationElement Element { get; }

    public RunTimeId RunTimeId => new(Element);

    public AutomationElementObject(AutomationElement element) => Element = element;
}
