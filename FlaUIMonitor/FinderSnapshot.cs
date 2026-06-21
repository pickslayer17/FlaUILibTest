namespace FlaUIMonitor;

/// <summary>
/// Иммутабельный слепок одного finder'а для отрисовки. Не держит ссылку на живой finder
/// и не обращается к UIA — только захваченные значения.
/// </summary>
public sealed record FinderSnapshot(string? Name, int[] RuntimeId)
{
    public string RuntimeIdText => RuntimeId is null ? "[]" : $"[{string.Join(",", RuntimeId)}]";
}
