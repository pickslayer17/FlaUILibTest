namespace UIDriver.Visualization;

internal static class Program
{
    private const string SingleInstanceMutexName = "UIDriver.Visualization.SingleInstance";

    [STAThread]
    private static void Main()
    {
        using var singleInstance = new Mutex(initiallyOwned: true, SingleInstanceMutexName, out var isFirstInstance);
        if (!isFirstInstance)
            return;

        ApplicationConfiguration.Initialize();
        Application.Run(new VisualizerApplicationContext());
    }
}
