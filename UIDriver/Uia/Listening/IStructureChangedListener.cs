using Interop.UIAutomationClient;

namespace UIDriver.Interfaces;

public interface IStructureChangedListener
{
    public void NotifyOnStructureChanged(IUIAutomationElement source, StructureChangeType changeType, int[] runtimeId);
}
