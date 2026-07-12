using FlaUI.Core.Definitions;

namespace UIDriver.Interfaces;

public interface IStructureChangedListener
{
    public void NotifyOnStructureChanged(UIAutomationElement source, StructureChangeType changeType, int[] runtimeId);
}
