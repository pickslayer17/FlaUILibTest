using System.Collections.Concurrent;
using UIDriver.Exceptions;
using UIDriver.Uia;

namespace UIDriver.Windows;

public sealed class WindowRegistry : IDisposable
{
    private readonly ConcurrentDictionary<RunTimeId, WindowContainer> _containers = new();

    public int ProcessId { get; set; }
    public WindowContainer? DefaultContainer { get; private set; }
    public WindowContainer? DesktopContainer { get; private set; }

    public void RegisterDefault(WindowContainer container)
    {
        Add(container);
        DefaultContainer = container;
    }

    public void RegisterDesktop(WindowContainer container)
    {
        Add(container);
        DesktopContainer = container;
    }

    public bool Contains(RunTimeId windowRunTimeId) => _containers.ContainsKey(windowRunTimeId);

    public void Add(WindowContainer container)
    {
        if (!_containers.TryAdd(container.WindowRunTimeId, container))
            throw new WindowRegistryException($"Failed to add window container for window [{container.WindowRunTimeId}].");
    }

    public void Remove(RunTimeId windowRunTimeId)
    {
        if (!_containers.TryRemove(windowRunTimeId, out var container))
            throw new WindowRegistryException("we have check on container exist, so its very strange that is wasnt removed");

        container.Dispose();

        if (!IsDefaultContainerExists())
            ReassignDefaultContainer();
    }

    public void Dispose()
    {
        foreach (var container in _containers.Values)
            container.Dispose();

        _containers.Clear();
    }

    private void ReassignDefaultContainer()
    {
        var allApplicationContainers = _containers.Where(kv => kv.Value != DesktopContainer).Where(kv => kv.Value.ProcessId == ProcessId);
        if (!allApplicationContainers.Any())
            throw new NotImplementedException("should be some logic, dont know which");

        DefaultContainer = allApplicationContainers.First().Value;
    }

    private bool IsDefaultContainerExists() => _containers.Any(kvp => ReferenceEquals(kvp.Value, DefaultContainer));
}
