using UIDriver.Tree.Snapshots;

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

    public void ClearAll()
    {
        if (IsDisposed)
            return;

        if (InvokeRequired)
        {
            BeginInvoke(() => ClearAll());
            return;
        }

        _tabControl.TabPages.Clear();
    }

    public void AddBranch(BranchSnapshot branch)
    {
        if (IsDisposed)
            return;

        if (InvokeRequired)
        {
            BeginInvoke(() => AddBranch(branch));
            return;
        }

        var treeView = new TreeView { Dock = DockStyle.Fill };
        treeView.Nodes.Add(TreeNodeBuilder.Build(branch.Root));
        treeView.ExpandAll();

        var page = new TabPage(FormatTitle(branch));
        page.Controls.Add(treeView);
        _tabControl.TabPages.Add(page);
        _tabControl.SelectedTab = page;
    }

    private static string FormatTitle(BranchSnapshot branch)
        => $"{branch.Origin} #{branch.Number} [{branch.TopRunTimeId?.ToHexString() ?? "no RID"}]";
}
