using System.Collections.Concurrent;
using FlaUI.Core.AutomationElements;

namespace UIDriver;

public sealed class Watcher
{
    private readonly ConcurrentDictionary<Guid, Watch> _watches = new();

    public Task<AutomationElement> AddWatch(IFinder finder, AutomationElement source, TimeSpan timeout)
        => throw new NotImplementedException();

    public void Poke() => throw new NotImplementedException();
}
