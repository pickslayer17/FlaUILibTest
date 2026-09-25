using System.Text.Json;

namespace UIDriver.Diagnostics.Remote;

public static class SnapshotJson
{
    private static readonly JsonSerializerOptions Options = new()
    {
        MaxDepth = 512,
        Converters = { new RunTimeIdJsonConverter() }
    };

    public static string Serialize(SnapshotMessage message) => JsonSerializer.Serialize(message, Options);

    public static SnapshotMessage Deserialize(string line)
        => JsonSerializer.Deserialize<SnapshotMessage>(line, Options)
            ?? throw new JsonException("Snapshot message is null.");
}
