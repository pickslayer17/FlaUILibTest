using System.Windows.Forms;
using UIDriver.CustomModels;

namespace UIDriver.Visualization;

public sealed class TreeVisualizerForm : Form
{
    private readonly TabControl _tabControl;
    private readonly Dictionary<object, TabPage> _pagesByOwner = new(ReferenceEqualityComparer.Instance);

    public TreeVisualizerForm()
    {
        Text = "UIDriver Tree Visualizer";
        Width = 900;
        Height = 1000;

        _tabControl = new TabControl { Dock = DockStyle.Fill };
        Controls.Add(_tabControl);
    }

}
