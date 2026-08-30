using System.Windows.Forms;
using UIDriver.CacheManagement;
using UIDriver.CustomModels;

namespace UIDriver.Visualization;

public sealed class TreeVisualizerForm : Form
{
    private readonly TabControl _tabControl;
    private readonly Dictionary<int, TabPage> _pagesByContainer = new();

    public TreeVisualizerForm()
    {
        Text = "UIDriver Tree Visualizer";
        Width = 900;
        Height = 1000;

        _tabControl = new TabControl { Dock = DockStyle.Fill };
        Controls.Add(_tabControl);
    }

    public void RenderSnapshot(ContainerId container, string title, TreeSnapshot snapshot)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => RenderSnapshot(container, title, snapshot));
            return;
        }

        var page = GetOrCreatePage(container);
        page.Text = string.IsNullOrEmpty(title) ? "(no title)" : title;

        page.Controls.Clear();

        var treeView = new TreeView { Dock = DockStyle.Fill };
        var root = BuildTreeNode(snapshot.Root);
        if (root != null)
            treeView.Nodes.Add(root);
        treeView.ExpandAll();

        page.Controls.Add(treeView);
        _tabControl.SelectedTab = page;
    }

    private TabPage GetOrCreatePage(ContainerId container)
    {
        if (_pagesByContainer.TryGetValue(container.Value, out var existing))
            return existing;

        var page = new TabPage();
        _tabControl.TabPages.Add(page);
        _pagesByContainer[container.Value] = page;
        return page;
    }

    private static TreeNode? BuildTreeNode(NodeSnapshot node)
    {
        if (node == null) return null;

        var label = $"[{ControlTypeName(node.ControlType)}] name='{node.Name}' [{node.RunTimeId.ToHexString()}]";
        if (node.ChangeState != NodeChangeState.Original)
            label += $" <{node.ChangeState}@{node.ChangedAtIteration}>";

        var treeNode = new TreeNode(label);

        foreach (var child in node.Children)
        {
            var childNode = BuildTreeNode(child);
            if (childNode != null)
                treeNode.Nodes.Add(childNode);
        }

        return treeNode;
    }

    private static string ControlTypeName(int controlType)
    {
        return Enum.IsDefined(typeof(UiaControlType), controlType)
            ? ((UiaControlType)controlType).ToString()
            : controlType.ToString();
    }
}
