using FlaUI.Core.AutomationElements;

namespace FlaUILibTest.Inspector;

// Самодостаточный отложенный поиск. Несёт СВОЮ попытку поиска (замкнутую на свой корень,
// условие и примитив поиска нужного finder'а) — поэтому координатору не нужно знать ни про
// окна, ни про то, КАК искать. Координатор только держит watch'и и пинает их (TryResolve).
public sealed class Watch
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Label { get; }

    private readonly Func<AutomationElement> _attempt;
    private readonly TaskCompletionSource<AutomationElement> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task<AutomationElement> Task => _tcs.Task;

    public Watch(string label, Func<AutomationElement> attempt)
    {
        Label = label;
        _attempt = attempt;
    }

    // Пытается зарезолвиться от своего корня. true => watch завершён (резолвнут либо уже был завершён).
    public bool TryResolve()
    {
        if (_tcs.Task.IsCompleted) return true;

        AutomationElement found;
        try { found = _attempt(); }
        catch { return false; } // окно могло закрыться/устареть — не роняем общий цикл Poke

        return found != null && _tcs.TrySetResult(found);
    }

    public void Cancel() => _tcs.TrySetCanceled();
}
