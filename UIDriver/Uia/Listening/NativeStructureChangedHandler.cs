using Interop.UIAutomationClient;

namespace UIDriver;

public sealed class NativeStructureChangedHandler : IUIAutomationStructureChangedEventHandler
{
    private readonly Action<IUIAutomationElement, StructureChangeType, int[]> _onStructureChanged;

    public NativeStructureChangedHandler(Action<IUIAutomationElement, StructureChangeType, int[]> onStructureChanged)
    {
        _onStructureChanged = onStructureChanged;
    }

    public void HandleStructureChangedEvent(IUIAutomationElement sender, StructureChangeType changeType, int[] runtimeId)
    {
        _onStructureChanged(sender, changeType, runtimeId);
    }
}
