using System.Windows.Forms;

namespace FlaUIMonitor;

/// <summary>
/// Запуск окна монитора. Поднимает форму на отдельном STA-потоке, чтобы не блокировать
/// консоль/драйвер. Зовётся вручную из Program.Main.
/// </summary>
public static class MonitorApp
{
    private static Thread? _thread;

    public static void Start()
    {
        if (_thread != null) return;

        _thread = new Thread(() =>
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MonitorForm());
        })
        {
            IsBackground = true
        };
        _thread.SetApartmentState(ApartmentState.STA);
        _thread.Start();
    }
}
