using UIDriver.Api;
using UIDriver.Diagnostics;
using UIDriver.Events;
using UIDriver.Exceptions;
using UIDriver.Tree;
using UIDriver.Uia;
using UIDriver.Uia.Listening;

namespace UIDriver.Windows;

public sealed class WindowContainer : IDisposable
{
    public string? WindowTitle { get; }
    public RunTimeId WindowRunTimeId { get; }
    public int ProcessId { get; }
    public UICachedTree CachedTree { get; }

    private readonly WindowListener _windowListener;

    public WindowContainer(UiaElement window, UiaAutomation automation, EventQueue eventQueue, SnapshotPublisher snapshotPublisher)
    {
        WindowTitle = window.Live.Name;
        WindowRunTimeId = window.Live.RunTimeId ?? throw new WindowRegistryException("Invalid window RuntimeId");
        ProcessId = window.Live.ProcessId;

        var cachedWindow = window.Live.BuildUpdatedCache(CacheProfile.Subtree);
        CachedTree = new UICachedTree(cachedWindow, WindowRunTimeId, WindowTitle, snapshotPublisher);
        CachedTree.PublishSnapshot();

        _windowListener = new WindowListener(window, WindowRunTimeId, automation);
        ForwardWindowEventsTo(eventQueue);
        _windowListener.StartListening();
    }

    public Task<UiaElement> SubmitOrderAsync(UIBy by) => throw new NotImplementedException();

    public void Dispose() => _windowListener.Dispose();

    private void ForwardWindowEventsTo(EventQueue eventQueue)
    {
        var forwarder = new WindowEventForwarder(CachedTree, eventQueue);
        _windowListener.RegisterStructureChangedListener(forwarder);
        _windowListener.RegisterPropertyChangedListener(forwarder);
        _windowListener.RegisterToggleWindowListener(forwarder);
    }
}
