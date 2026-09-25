using CacheManagement;
using Interop.UIAutomationClient;
using UIDriver.CustomModels;

namespace UIDriver.NewCacheManagement;

public class NodeFactory
{
    public static UiNode NewNode(IUIAutomationElement uiAutomationElement, bool liveRunTimeId = false)
    {
        var runTimeId = liveRunTimeId? 
            uiAutomationElement.LiveRuntimeId().ToCacheRunTimeId() :
            uiAutomationElement.CachedRuntimeId();

        var node = new UiNode(uiAutomationElement)
        {
            RunTimeId = runTimeId,
        };

        return node;
    }

    public static UiNode NewNodeWithParent(
        IUIAutomationElement uiAutomationElement,
        UiNode parentNode,
        bool liveRunTimeId = false)
    {
        var node = new UiNode(uiAutomationElement);
        node.Parent = parentNode;

        return node;
    }
}
