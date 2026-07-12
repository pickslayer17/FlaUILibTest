using System.Windows.Forms;

namespace UIDriver.Visualization;

public sealed class TreeVisualizerForm : Form
{
    private readonly TabControl _tabControl;

    public TreeVisualizerForm()
    {
        Text = "UIDriver Tree Visualizer";
        Width = 900;
        Height = 1000;

        _tabControl = new TabControl { Dock = DockStyle.Fill };
        Controls.Add(_tabControl);
    }

    public void Render(IReadOnlyList<(string Title, UiNode Tree)> containers)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => Render(containers));
            return;
        }

        _tabControl.TabPages.Clear();

        foreach (var (title, tree) in containers)
        {
            var page = new TabPage(string.IsNullOrEmpty(title) ? "(no title)" : title);
            var treeView = new TreeView { Dock = DockStyle.Fill };

            var root = BuildTreeNode(tree);
            if (root != null)
                treeView.Nodes.Add(root);
            treeView.ExpandAll();

            page.Controls.Add(treeView);
            _tabControl.TabPages.Add(page);
        }
    }

    private static TreeNode? BuildTreeNode(UiNode node)
    {
        if (node == null) return null;

        var label = $"[{node.ControlType}] name='{node.Name}' class='{node.ClassName}' aid='{node.AutomationId}'";
        var treeNode = new TreeNode(label);

        foreach (var child in node.Children ?? [])
        {
            var childNode = BuildTreeNode(child);
            if (childNode != null)
                treeNode.Nodes.Add(childNode);
        }

        return treeNode;
    }
}
