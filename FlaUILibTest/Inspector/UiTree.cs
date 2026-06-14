using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using System.Xml.Linq;

namespace FlaUILibTest.Inspector;


public class UITree
{
    public int treeIteration = 0;
    public TreeNode Root { get; private set; }
    private readonly List<ConditionBase> _watchConditions = new();
    private readonly object _treeLock = new();

    public UITree(AutomationElement rootElement)
    {
        Root = new TreeNode(rootElement, null, 0);
    }

    public void WatchElement(ConditionBase condition)
    {
        _watchConditions.Add(condition);
    }

    private void CheckWatches(AutomationElement element, TreeNode node, int[] eventRuntimeId)
    {
        foreach (var condition in _watchConditions)
        {
            if (ConditionMatcher.Matches(element, condition))
            {
                Console.WriteLine($">>> FOUND: {element.Properties.Name.ValueOrDefault}");
                Console.WriteLine($">>> FOUND: {element.Properties.Name.ValueOrDefault} | eventRid: [{(eventRuntimeId == null ? "null" : string.Join(",", eventRuntimeId))}]");
                var parent = node;
                while (parent != null)
                {
                    var mark = "";
                    if (eventRuntimeId != null)
                    {
                        try
                        {
                            var parentRunTimeId = parent.Element.Properties.RuntimeId.ValueOrDefault;
                            if (parentRunTimeId != null && parentRunTimeId.SequenceEqual(eventRuntimeId))
                                mark = " <<< EVENT";
                        }
                        catch { }
                    }
                    Console.WriteLine($"  <- {parent.Name} | {parent.ClassName} | {parent.ControlType}{mark}");
                    parent = parent.Parent;
                }
            }
        }
    }

    public async Task BuildAsync()
    {
        await Task.Run(() =>
        {
            lock (_treeLock)
            {
                BuildRecursive(Root, null);
            }
        });
    }

    public void SubscribeToEvents(Window window)
    {
        window.RegisterStructureChangedEvent(
            TreeScope.Subtree,
            OnStructureChanged);
    }

    public void PrintLevels()
    {
        var levels = new Dictionary<int, int>();
        CountLevels(Root, levels);
        Console.WriteLine($"[[[ITERTATION - {treeIteration++}]]]");
        foreach (var kv in levels.OrderBy(x => x.Key))
        {
            Console.WriteLine($"[{kv.Value}]");
        }
        Console.WriteLine("---");
    }

    private void CountLevels(TreeNode node, Dictionary<int, int> levels)
    {
        if (!levels.ContainsKey(node.Depth))
            levels[node.Depth] = 0;
        levels[node.Depth]++;

        foreach (var child in node.Children)
            CountLevels(child, levels);
    }

    private void OnStructureChanged(AutomationElement element, StructureChangeType changeType, int[] runtimeId)
    {
        lock (_treeLock)
        {
            switch (changeType)
            {
                case StructureChangeType.ChildAdded:
                    HandleChildAdded(element);
                    break;
                case StructureChangeType.ChildRemoved:
                    HandleChildRemoved(runtimeId);
                    break;
            }

            //PrintLevels();
        }
    }

    private void BuildRecursive(TreeNode node, int[] eventRuntimeId)
    {
        AutomationElement[] children;
        try
        {
            children = node.Element.FindAllChildren();
        }
        catch { return; }

        foreach (var child in children)
        {
            try
            {
                var childNode = new TreeNode(child, node, node.Depth + 1);
                node.AddChild(childNode);
                CheckWatches(child, node, eventRuntimeId);
                BuildRecursive(childNode, eventRuntimeId);
            }
            catch { continue; }
        }
    }

    private void HandleChildAdded(AutomationElement element)
    {
        try
        {
            var runTimeId = element.Properties.RuntimeId.ValueOrDefault;
            if (runTimeId == null) return;

            var parent = element.Parent;
            if (parent == null) return;

            var parentRid = parent.Properties.RuntimeId.ValueOrDefault;
            if (parentRid == null) return;

            var parentNode = FindNodeByRuntimeId(Root, parentRid);
            if (parentNode == null) return;

            var existing = FindNodeByRuntimeId(parentNode, runTimeId);
            if (existing != null) return;

            var newNode = new TreeNode(element, parentNode, parentNode.Depth + 1);
            parentNode.AddChild(newNode);
            CheckWatches(element, parentNode, runTimeId);
            BuildRecursive(newNode, runTimeId);
        }
        catch { }
    }

    private void HandleChildRemoved(int[] runtimeId)
    {
        if (runtimeId == null || runtimeId.Length == 0) return;

        try
        {
            var node = FindNodeByRuntimeId(Root, runtimeId);
            if (node == null) return;

            node.Parent?.Children.Remove(node);
        }
        catch { }
    }

    private TreeNode? FindNodeByRuntimeId(TreeNode root, int[] runtimeId)
    {
        try
        {
            var nodeRid = root.Element.Properties.RuntimeId.ValueOrDefault;
            if (nodeRid != null && nodeRid.SequenceEqual(runtimeId))
                return root;
        }
        catch { }

        foreach (var child in root.Children)
        {
            var found = FindNodeByRuntimeId(child, runtimeId);
            if (found != null) return found;
        }

        return null;
    }
}