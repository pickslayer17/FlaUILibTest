using System.IO.Pipes;
using UIDriver.Diagnostics.Remote;

namespace UIDriver.Visualization;

internal sealed class SnapshotPipeServer
{
    private readonly Action _onSessionStarted;
    private readonly Action<SnapshotMessage> _onMessageReceived;

    public SnapshotPipeServer(Action onSessionStarted, Action<SnapshotMessage> onMessageReceived)
    {
        _onSessionStarted = onSessionStarted;
        _onMessageReceived = onMessageReceived;
    }

    public void Start()
    {
        var thread = new Thread(ServeSessions)
        {
            IsBackground = true,
            Name = "Snapshot pipe server"
        };
        thread.Start();
    }

    private void ServeSessions()
    {
        while (true)
            ServeSession();
    }

    private void ServeSession()
    {
        using var pipe = new NamedPipeServerStream(SnapshotPipe.Name, PipeDirection.In, maxNumberOfServerInstances: 1);
        pipe.WaitForConnection();
        _onSessionStarted();

        using var reader = new StreamReader(pipe);
        while (reader.ReadLine() is { } line)
            _onMessageReceived(SnapshotJson.Deserialize(line));
    }
}
