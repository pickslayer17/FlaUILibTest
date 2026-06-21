namespace FlaUILibTest.Helpers;

public static class LogManager
{
    private static readonly Lock _logLock = new();

    public static void Log(string message, [System.Runtime.CompilerServices.CallerFilePath] string filePath = "", [System.Runtime.CompilerServices.CallerMemberName] string caller = "")
    {
        lock (_logLock)
        {
            Console.WriteLine($"{CreateLogMessage(message, filePath, caller)}");
        }
    }

    public static void LogError(string message, [System.Runtime.CompilerServices.CallerFilePath] string filePath = "", [System.Runtime.CompilerServices.CallerMemberName] string caller = "")
    {
        lock (_logLock)
        {
            Console.Error.WriteLine($"!!!!!<<<ERROR>>>!!!!!\n\n{CreateLogMessage(message,filePath,caller)}\n\n!!!!!<<<ERROR>>>!!!!!");
        }
    }

    private static string CreateLogMessage(string message, [System.Runtime.CompilerServices.CallerFilePath] string filePath = "", [System.Runtime.CompilerServices.CallerMemberName] string caller = "")
    {
        var timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var fileName = Path.GetFileNameWithoutExtension(filePath);

        return $"[{timeStamp}]-[{fileName}]::[{caller}] {message}";
    }
}