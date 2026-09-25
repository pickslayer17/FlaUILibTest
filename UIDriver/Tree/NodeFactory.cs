using UIDriver.Uia;

namespace UIDriver.Tree;

public static class NodeFactory
{
    public static UiNode NewNodeFromCache(UiaElement element)
        => new(element, element.Cached.RunTimeId, element.Cached.ControlType, element.Cached.Name);

    public static UiNode NewHeelFromLive(UiaElement liveParent)
        => new(liveParent, liveParent.Live.RunTimeId, liveParent.Live.ControlType, liveParent.Live.Name);
}
