using UIDriver.Tree.Snapshots;
using UIDriver.Uia;

namespace UIDriver.Visualization;

public sealed class TreeVisualizerForm : Form
{
    private readonly TabControl _tabControl;
    private readonly Dictionary<RunTimeId, TreeView> _treeViewsByWindow = new();

    public TreeVisualizerForm()
    {
        Text = "UIDriver Tree Visualizer";
        Width = 900;
        Height = 1000;

        _tabControl = new TabControl { Dock = DockStyle.Fill };
        Controls.Add(_tabControl);
    }

    public void RenderSnapshot(TreeSnapshot snapshot)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => RenderSnapshot(snapshot));
            return;
        }

        var treeView = GetOrCreateTreeView(snapshot.WindowRunTimeId);
        FillTreeView(treeView, snapshot);

        var page = (TabPage)treeView.Parent!;
        page.Text = $"{snapshot.WindowTitle} v{snapshot.Version}";
        _tabControl.SelectedTab = page;
    }

    private static void FillTreeView(TreeView treeView, TreeSnapshot snapshot)
    {
        treeView.BeginUpdate();
        treeView.Nodes.Clear();
        treeView.Nodes.Add(TreeNodeBuilder.Build(snapshot.Root, snapshot.Version));
        treeView.ExpandAll();
        treeView.EndUpdate();
    }

    private TreeView GetOrCreateTreeView(RunTimeId windowRunTimeId)
    {
        if (_treeViewsByWindow.TryGetValue(windowRunTimeId, out var existing))
            return existing;

        var treeView = new TreeView { Dock = DockStyle.Fill };
        var page = new TabPage();
        page.Controls.Add(treeView);
        _tabControl.TabPages.Add(page);
        _treeViewsByWindow[windowRunTimeId] = treeView;

        return treeView;
    }
}
