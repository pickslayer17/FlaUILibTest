using System.Windows.Forms;
using UIDriver.CustomModels;
using UIDriver.NewCacheManagement;

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
      
    }
}
