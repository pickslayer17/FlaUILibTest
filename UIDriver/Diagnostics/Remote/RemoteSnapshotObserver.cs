using System.Diagnostics;
using System.IO.Pipes;
using UIDriver.Tree.Snapshots;

namespace UIDriver.Diagnostics.Remote;

public sealed class RemoteSnapshotObserver : ITreeObserver, IBranchObserver, IDisposable
{
    private const int RunningVisualizerTimeoutMilliseconds = 300;
    private const int StartingVisualizerTimeoutMilliseconds = 10_000;

    private readonly StreamWriter _writer;
    private readonly object _writeLock = new();
    private bool _isVisualizerClosed;

    public RemoteSnapshotObserver(string visualizerExecutablePath)
    {
        var pipe = ConnectToVisualizer(visualizerExecutablePath);
        _writer = new StreamWriter(pipe) { AutoFlush = true };
    }

    public void OnTreeSnapshot(TreeSnapshot snapshot) => Send(new TreeSnapshotMessage(snapshot));

    public void OnBranchSnapshot(BranchSnapshot snapshot) => Send(new BranchSnapshotMessage(snapshot));

    public void Dispose() => _writer.Dispose();

    private void Send(SnapshotMessage message)
    {
        var line = SnapshotJson.Serialize(message);
        lock (_writeLock)
        {
            if (_isVisualizerClosed)
                return;

            try
            {
                _writer.WriteLine(line);
            }
            catch (IOException)
            {
                _isVisualizerClosed = true;
            }
        }
    }

    private static NamedPipeClientStream ConnectToVisualizer(string visualizerExecutablePath)
    {
        var runningVisualizerPipe = TryConnect(RunningVisualizerTimeoutMilliseconds);
        if (runningVisualizerPipe != null)
            return runningVisualizerPipe;

        Process.Start(visualizerExecutablePath);

        return TryConnect(StartingVisualizerTimeoutMilliseconds)
            ?? throw new TimeoutException($"Visualizer [{visualizerExecutablePath}] did not open pipe [{SnapshotPipe.Name}].");
    }

    private static NamedPipeClientStream? TryConnect(int timeoutMilliseconds)
    {
        var pipe = new NamedPipeClientStream(".", SnapshotPipe.Name, PipeDirection.Out);
        try
        {
            pipe.Connect(timeoutMilliseconds);
            return pipe;
        }
        catch (TimeoutException)
        {
            pipe.Dispose();
            return null;
        }
    }
}
