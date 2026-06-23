using FlaUI.Core.AutomationElements;

namespace UIDriver;

public sealed class AutomationElementObject
{
    public AutomationElement Element { get; }

    public RunTimeId RunTimeId => new(Element.Properties.RuntimeId.ValueOrDefault!);

    public AutomationElementObject(AutomationElement element) => Element = element;
}
