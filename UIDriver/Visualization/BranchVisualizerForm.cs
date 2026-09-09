using System.Windows.Forms;
using UIDriver.CustomModels;

namespace UIDriver.Visualization;

public sealed class BranchVisualizerForm : Form
{
    private readonly TabControl _tabControl;

    public BranchVisualizerForm()
    {
        Text = "UIDriver Branch Visualizer";
        Width = 900;
        Height = 1000;

        _tabControl = new TabControl { Dock = DockStyle.Fill };
        Controls.Add(_tabControl);
    }

    public void AddBranch(string title, NodeSnapshot branch)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => AddBranch(title, branch));
            return;
        }

        var page = new TabPage(string.IsNullOrEmpty(title) ? "(no title)" : title);
        var treeView = new TreeView { Dock = DockStyle.Fill };

        var root = BuildTreeNode(branch);
        if (root != null)
            treeView.Nodes.Add(root);
        treeView.ExpandAll();

        page.Controls.Add(treeView);
        _tabControl.TabPages.Add(page);
        _tabControl.SelectedTab = page;
    }

    private static TreeNode? BuildTreeNode(NodeSnapshot node)
    {
        if (node == null) return null;

        var label = $"[{ControlTypeName(node.ControlType)}] name='{node.Name}' [{node.RunTimeId.ToHexString()}]";
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
