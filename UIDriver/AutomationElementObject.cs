using FlaUI.Core.AutomationElements;

namespace UIDriver;

public sealed class AutomationElementObject
{
    public AutomationElement Element { get; }

    public RunTimeId RunTimeId 
    { 
        get 
        {
            int[] runtimeId = [-1];
            try 
            {
                runtimeId = Element.Properties.RuntimeId.ValueOrDefault ?? [-2];
            }
            catch (Exception ex) 
            {
                LogEventFactory.RaiseText($"Failed to get RuntimeId for element: {ex.Message}");
            }

            return new(runtimeId);
        } 
    }

    public AutomationElementObject(AutomationElement element) => Element = element;
}
