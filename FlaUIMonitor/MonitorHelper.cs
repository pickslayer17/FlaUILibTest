namespace FlaUIMonitor;

/// <summary>
/// Центральный хаб мониторинга. Ядро (FinderManager) пушит сюда слепок при каждом изменении
/// реестра finder'ов через <see cref="Publish"/>; UI кооперируется только с этим классом:
/// читает <see cref="Current"/> при открытии и подписывается на <see cref="SnapshotChanged"/>.
/// Статик — намеренно: единая точка, без проброса ссылок.
/// </summary>
public static class MonitorHelper
{
    private static readonly Lock _lock = new();
    private static IReadOnlyList<FinderSnapshot> _current = Array.Empty<FinderSnapshot>();

    /// <summary>Срабатывает после каждого Publish. Может прийти из не-UI потока — подписчик сам маршалит.</summary>
    public static event Action<IReadOnlyList<FinderSnapshot>>? SnapshotChanged;

    public static IReadOnlyList<FinderSnapshot> Current
    {
        get { lock (_lock) { return _current; } }
    }

    public static void Publish(IReadOnlyList<FinderSnapshot> snapshot)
    {
        snapshot ??= Array.Empty<FinderSnapshot>();
        lock (_lock) { _current = snapshot; }
        SnapshotChanged?.Invoke(snapshot);
    }
}
