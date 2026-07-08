class UiNodeWalker
{
    public UiNode MoveParent(UiNode uiNode)
    {
        var parent = uiNode.Parent;
        if(parent != null)
            return parent;

        return null;
    }

    public UiNode MoveFirstChild(UiNode uiNode)
    {
        var children = uiNode.Children;
        if(children != null && children.Length > 0)
            return children[0];

        return null;
    }

    public UiNode MovePrevSibling(UiNode uiNode)
    {
        var siblings = GetSiblings(uiNode);
        var siblingIndex = GetSiblingIndex(uiNode);

        if(siblingIndex != 0)
            return siblings[siblingIndex - 1];

        return null;
    }

    public UiNode MoveNextSibling(UiNode uiNode)
    {
        var siblings = GetSiblings(uiNode);
        var siblingIndex = GetSiblingIndex(uiNode);

        if(siblingIndex != siblings.Length - 1)
            return siblings[siblingIndex + 1];

        return null;
    }

    int GetSiblingIndex(UiNode uiNode)
    {
        int siblingIndex = 0;
        var siblings = GetSiblings(uiNode);
        for(int i = 0; i< siblings.Length; i++)
        {
            if(siblings[i] == uiNode)
            {
                 siblingIndex=i;
                 break;
            }
        }

        return siblingIndex;
    }

    UiNode[] GetSiblings(UiNode uiNode)
    {
        var parent = uiNode.Parent;
        if(parent == null)
            throw new Exception("oppa nihuya");

        return parent.Children;
    }
}
