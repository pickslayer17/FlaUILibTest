using UIDriver;
using FlaUI.Core.Conditions;
using FlaUILibTest;

class UISuperFinder
{
    readonly UiNode _desktop;
    readonly UiNodeWalker _walker;

    public UISuperFinder(UiNode desktop, UiNodeWalker walker)
    {
        _desktop = desktop;
        _walker = walker;
    }

    public IEnumerable<UiNode> FindAll(UiNode root, ConditionBase condition)
    {
        if (root == null) yield break;

        if (CheckProperty(root, condition))
            yield return root;

        var child = _walker.MoveFirstChild(root);
        while (child != null)
        {
            foreach (var found in FindAll(child, condition))
                yield return found;

            child = _walker.MoveNextSibling(child);
        }
    }

    public IEnumerable<UiNode> FindChildren(UiNode root, ConditionBase condition)
    {
        if (root == null) yield break;

        var child = _walker.MoveFirstChild(root);
        while (child != null)
        {
            if (CheckProperty(child, condition))
                yield return child;

            child = _walker.MoveNextSibling(child);
        }
    }

    public bool CheckProperty(UiNode node, ConditionBase condition)
    {
        var result = new UiNodePropMatcher(condition).Matches(node);
        return result;
    }

    public UiNode Find(UIBy targetBy)
    {
        var stepStack = BuildStepStack(targetBy, out var scope);
        // detect root
        var root = scope == WindowScope.Desktop ? _desktop : null;

        // defender from null
        if (stepStack.Count > 0 && stepStack.All(step => step != null))
            return Search(root, stepStack);

        return null;
    }

    UiNode Search(UiNode source, Stack<UIBy> stepStack)
    {
        // stack has something? - go further! stack is empty? - that means all previous check steps were passed - it is the element we are looking for.
        var stepBy = stepStack.Count > 0 ? stepStack.Pop() : null;
        if (stepBy == null)
            return source;

        // real search from root by conditions. e.g. recurscive going down and checking selfCondition with each element until its not equals.
        var candidates = stepBy.IsChild ? FindChildren(source, stepBy.SelfCondition) : FindAll(source, stepBy.SelfCondition);
        // self conditions are fine. It's time to check whether his all relative conditions are ok.
        foreach (var found in candidates)
        {
            var relationsOk = CheckRelations(found, stepBy);
            if (relationsOk)
            {
                var deeper = Search(found, stepStack);
                if (deeper != null)
                    return deeper;
            }
        }

        return null;
    }

    // checking all except SelfConditions. If anything is false - return false and break whole search
    // take real element and walk through all relations
    bool CheckRelations(UiNode element, UIBy by)
    {
        if (!CheckSiblingIndex(element, by)) return false;

        foreach (var precedingSiblingBy in by.PrecedingSiblings ?? [])
        {
            if (!CheckPrecedingSibling(element, precedingSiblingBy)) return false;
        }

        foreach (var followingSiblingBy in by.FollowingSiblings ?? [])
        {
            if (!CheckFollowingSibling(element, followingSiblingBy)) return false;
        }

        foreach (var childBy in by.Children ?? [])
        {
            if (!CheckChild(element, childBy)) return false;
        }

        foreach (var descendantBy in by.Descendants ?? [])
        {
            if (!CheckDescendant(element, descendantBy)) return false;
        }

        foreach (var followingBy in by.Following ?? [])
        {
            if (!CheckFollowing(element, followingBy)) return false;
        }

        foreach (var precedingBy in by.Preceding ?? [])
        {
            if (!CheckPreceding(element, precedingBy)) return false;
        }

        return true;
    }

    bool CheckElement(UiNode element, UIBy by)
    {
        var selfConditionOk = CheckProperty(element, by.SelfCondition);
        if (selfConditionOk)
        {
            var relationsOk = CheckRelations(element, by);
            if (relationsOk)
            {
                return true;
            }
        }

        return false;
    }

    bool CheckSiblingIndex(UiNode element, UIBy by)
    {
        if (by.SiblingIndex == null)
            return true;

        var previousSibling = element;
        var siblingIndex = by.SiblingIndex;
        while (previousSibling != null && siblingIndex != 0)
        {
            previousSibling = _walker.MovePrevSibling(previousSibling);
            siblingIndex--;
        }

        return previousSibling == null && siblingIndex == 0;
    }

    bool CheckPrecedingSibling(UiNode element, UIBy precedingSiblingBy)
    {
        var precedingSibling = _walker.MovePrevSibling(element);
        while (precedingSibling != null)
        {
            var elementFits = CheckElement(precedingSibling, precedingSiblingBy);
            if (elementFits)
                return true;

            precedingSibling = _walker.MovePrevSibling(precedingSibling);
        }
        return false;
    }

    bool CheckFollowingSibling(UiNode element, UIBy followingSiblingBy)
    {
        var followingSibling = _walker.MoveNextSibling(element);
        while (followingSibling != null)
        {
            var elementFits = CheckElement(followingSibling, followingSiblingBy);
            if (elementFits)
                return true;

            followingSibling = _walker.MoveNextSibling(followingSibling);
        }
        return false;
    }

    bool CheckChild(UiNode element, UIBy childBy)
    {
        var child = _walker.MoveFirstChild(element);
        while (child != null)
        {
            if (CheckElement(child, childBy))
            {
                return true;
            }
            else
            {
                child = _walker.MoveNextSibling(child);
            }
        }

        return false;
    }

    bool CheckDescendant(UiNode element, UIBy descendantBy)
    {
        // we already now the parent, we dont need all parents. but the function works with stack. so i dont see any problem
        var descendantStack = new Stack<UIBy>();
        descendantStack.Push(descendantBy);
        var found = Search(element, descendantStack);

        return found != null;
    }

    // these 2 guys are completely different.
    // following - getting all following-siblings and perform Search on each. After return to where we start -> go to parent -> moveNextSibling -> perform Search on it
    bool CheckFollowing(UiNode element, UIBy followingBy)
    {
        UiNode found = null;

        var following = _walker.MoveNextSibling(element);
        while (following != null)
        {
            if (following != null)
            {
                // sibling is a part of 'following' , so check it as well
                if (CheckElement(following, followingBy)) return true;

                // while we still inside siblings - perform without parent realtions
                found = SearchSingleStep(following, followingBy);
                if (found != null)
                    return true;

                following = _walker.MoveNextSibling(following);
            }
        }

        // if sibling subtrees didnt find anything - second part
        var parent = _walker.MoveParent(element);
        if (parent == null)
            return false;

        var parentFollowingSibling = _walker.MoveNextSibling(parent);
        if (parentFollowingSibling == null)
            return false;

        found = SearchFromRoot(parentFollowingSibling, followingBy);

        return found != null;
    }

    // seems like the same mechanism as above,
    bool CheckPreceding(UiNode element, UIBy precedingBy)
    {
        UiNode found = null;

        var preceding = _walker.MovePrevSibling(element);
        while (preceding != null)
        {
            if (preceding != null)
            {
                // sibling is a part of 'following' , so check it as well
                if (CheckElement(preceding, precedingBy)) return true;

                // while we still inside siblings - perform without parent realtions
                found = SearchSingleStep(preceding, precedingBy);
                if (found != null)
                    return true;

                preceding = _walker.MovePrevSibling(preceding);
            }
        }

        // if sibling subtrees didnt find anything - second part
        var parent = _walker.MoveParent(element);
        if (parent == null)
            return false;

        var parentPreviousSibling = _walker.MovePrevSibling(parent);
        if (parentPreviousSibling == null)
            return false;

        found = SearchFromRoot(parentPreviousSibling, precedingBy);

        return found != null;
    }

    // we know our element if final
    UiNode SearchSingleStep(UiNode source, UIBy by)
    {
        var stepStack = new Stack<UIBy>();
        stepStack.Push(by);
        var found = Search(source, stepStack);

        return found;
    }

    // our element could have ancestors
    UiNode SearchFromRoot(UiNode source, UIBy by)
    {
        var stepStack = BuildStepStack(by, out var scope);

        // detect root. custom for following/preceding elements
        UiNode root = null;
        if (scope == WindowScope.Desktop)
            root = _desktop;
        if (scope == WindowScope.Custom)
            root = source;

        // defender from null
        if (stepStack.Count > 0 && stepStack.All(step => step != null))
            return Search(root, stepStack);

        return null;
    }

    Stack<UIBy> BuildStepStack(UIBy by, out WindowScope scope)
    {
        var stepStack = new Stack<UIBy>();
        var current = by;
        scope = by.Scope;
        while (current != null)
        {
            stepStack.Push(current);
            scope = current.Scope;
            current = current.AncestorOrParent;
        }

        return stepStack;
    }
}
