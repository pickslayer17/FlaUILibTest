using Interop.UIAutomationClient;
using UIDriver;
using UIDriver.Constants;
using UIDriver.CustomModels;
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
        var cacheRequest = CacheRequestFactory.BuildCacheRequest(_automation);
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
        var sourceRid = addedChild.CachedRuntimeId();
        if (sourceRid.State != RunTimeIdStates.Valid)
        {
            return;
        }


        // Get real parent to understand where element was added
        var liveParentElement = _automation.RawViewWalker.GetParentElement(addedChild);
        if (liveParentElement == null)
            throw new NullReferenceException();

        

        

        //var heel = _cachedTree.GetNode(n => n.RunTimeId.Id.RuntimeIdEquals(parentRid.Id));
        //if (heel == null)
        //    throw new InvalidOperationException($"ADDED: heel [{parentRid.ToHexString()}] not found in cached tree");

        //_cachedTree.Add(heel, addedChildTree, ++_iteration, $"ADDED #{_collectedBranches.Count} [{parentRid.ToHexString()}]");
    }

    private void HandleChildrenInvalidated(IUIAutomationElement invalidatedParent)
    {
        var sourceRID = invalidatedParent.GetCachedPropertyValue((int)UiaProperty.RuntimeId) as int[];
    }
}