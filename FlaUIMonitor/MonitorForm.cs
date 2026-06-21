using System.Drawing;
using System.Windows.Forms;

namespace FlaUIMonitor;

/// <summary>
/// Примитивное окно мониторинга: по одному label на finder — "Finder ... — RuntimeId ...".
/// Кооперируется только с <see cref="MonitorHelper"/>: читает Current на старте, перерисовывается на SnapshotChanged.
/// </summary>
public sealed class MonitorForm : Form
{
    private readonly FlowLayoutPanel _panel;
    private readonly Label _header;

    public MonitorForm()
    {
        Text = "Finder Monitor";
        Width = 640;
        Height = 420;

        _header = new Label
        {
            AutoSize = true,
            Font = new Font("Consolas", 10, FontStyle.Bold),
            Padding = new Padding(4)
        };

        _panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(6)
        };

        Controls.Add(_panel);

        Load += (_, _) => Render(MonitorHelper.Current);
        MonitorHelper.SnapshotChanged += OnSnapshotChanged;
        FormClosed += (_, _) => MonitorHelper.SnapshotChanged -= OnSnapshotChanged;
    }

    private void OnSnapshotChanged(IReadOnlyList<FinderSnapshot> snapshot)
    {
        if (IsDisposed) return;

        // Publish может прийти из UIA-потока — маршалим в UI-поток формы.
        if (InvokeRequired)
        {
            BeginInvoke((Action)(() => Render(snapshot)));
            return;
        }

        Render(snapshot);
    }

    private void Render(IReadOnlyList<FinderSnapshot> snapshot)
    {
        _panel.SuspendLayout();
        _panel.Controls.Clear();

        _header.Text = $"Finders: {snapshot.Count}";
        _panel.Controls.Add(_header);

        foreach (var f in snapshot)
        {
            _panel.Controls.Add(new Label
            {
                AutoSize = true,
                Font = new Font("Consolas", 9),
                Margin = new Padding(2),
                Text = $"Finder \"{f.Name}\"  —  RuntimeId {f.RuntimeIdText}"
            });
        }

        _panel.ResumeLayout();
    }
}
