using System.Text.Json;
using System.Text.Json.Serialization;
using UIDriver.Uia;

namespace UIDriver.Diagnostics.Remote;

public sealed class RunTimeIdJsonConverter : JsonConverter<RunTimeId>
{
    public override RunTimeId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var id = JsonSerializer.Deserialize<int[]>(ref reader, options);
        return RunTimeId.FromArray(id) ?? throw new JsonException("RuntimeId in snapshot message is empty.");
    }

    public override void Write(Utf8JsonWriter writer, RunTimeId value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, value.ToArray(), options);
}
