using Interop.UIAutomationClient;
using UIDriver;
using UIDriver.Interfaces;
using UIDriver.NewCacheManagement;

namespace CacheManagement;

public class UICachedTreeManager : IStructureChangedListener, IPropertyChangedListener
{
    private IUIAutomation _automation;
    private UICachedTree _cachedTree;

    public UICachedTreeManager(IUIAutomation automation)
    {
        _automation = automation;
    }

    public void InitCachedTree(IUIAutomationElement window)
    {
        var cacheRequest = _automation.BuildCacheRequest();
        var _cachedWindow = window.BuildUpdatedCache(cacheRequest);

        _cachedTree = new UICachedTree(_cachedWindow);
    }

    public Lock notifyLock = new Lock();
    public void NotifyOnStructureChanged(IUIAutomationElement source, StructureChangeType changeType, int[] runtimeId)
    {
        lock (notifyLock)
        {
            switch (changeType)
            {
                case StructureChangeType.StructureChangeType_ChildAdded:
                    HandleChildAdded(source);
                    break;
                case StructureChangeType.StructureChangeType_ChildRemoved:
                    Console.WriteLine("REMOVED");
                    break;
                case StructureChangeType.StructureChangeType_ChildrenInvalidated:
                    HandleChildrenInvalidated(source);
                    break;
                case StructureChangeType.StructureChangeType_ChildrenReordered:
                    Console.WriteLine("REORDERED");
                    break;
                case StructureChangeType.StructureChangeType_ChildrenBulkRemoved:
                    Console.WriteLine("BULK_REMOVED");
                    break;
                case StructureChangeType.StructureChangeType_ChildrenBulkAdded:
                    Console.WriteLine("BULK_ADDED");
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public void NotifyOnPropertyChanged(IUIAutomationElement source, int propertyId, object newValue)
    {

    }

    private void HandleChildAdded(IUIAutomationElement addedChild)
    {

    }

    private void HandleChildrenInvalidated(IUIAutomationElement invalidatedParent)
    {
        var sourceRID = invalidatedParent.GetCachedPropertyValue((int)UiaProperty.RuntimeId) as int[];
    }
}