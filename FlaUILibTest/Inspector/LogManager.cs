namespace FlaUILibTest.Inspector;

public static class LogManager
{
    private static readonly Lock _logLock = new();
    public static void Log(string source, string message)
    {
        lock (_logLock)
        {
            Console.WriteLine($"[{source}] {message}");
        }
    }
}