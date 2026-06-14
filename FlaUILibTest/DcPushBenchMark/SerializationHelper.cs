using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Globalization;
using System.Text;
using System.Xml.Serialization;

namespace FlaUILibTest.DcPushBenchMark;

public class SerializationHelper
{
    public static Dictionary<string, string> DeserializeToDictionary(object metaToken)
    {
        if (metaToken == null)
        {
            return null;
        }

        if (!(metaToken is JToken token))
        {
            var serializer = new JsonSerializer { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

            return DeserializeToDictionary(JObject.FromObject(metaToken, serializer));
        }

        if (token.HasValues)
        {
            var contentData = new Dictionary<string, string>();

            return token.Children()
                .ToList()
                .Select(DeserializeToDictionary)
                .Where(childContent => childContent != null)
                .Aggregate(contentData,
                    (current, childContent) => current.Concat(childContent)
                        .ToDictionary(k => k.Key, v => v.Value));
        }

        var jValue = token as JValue;
        if (jValue?.Value == null)
        {
            return null;
        }

        var value = jValue.Type == JTokenType.Date
            ? jValue.ToString("o", CultureInfo.InvariantCulture)
            : jValue.ToString(CultureInfo.InvariantCulture);

        return new Dictionary<string, string> { { token.Path, value } };
    }

    public static T DeserializeXml<T>(string xmlString)
    {
        if (string.IsNullOrWhiteSpace(xmlString))
        {
            return default;
        }

        var serializer = new XmlSerializer(typeof(T));

        using (var reader = new StringReader(xmlString))
        {
            return (T)serializer.Deserialize(reader);
        }
    }

    public static string SerializeXml<T>(T instance)
    {
        var serializer = new XmlSerializer(typeof(T));
        var sb = new StringBuilder();

        using (var writer = new StringWriter(sb, CultureInfo.InvariantCulture))
        {
            serializer.Serialize(writer, instance);
        }

        return sb.ToString();
    }

    public static T DeserializeNewtonsoftJson<T>(string json, bool useAutoTypeHandling = false)
    {
        return useAutoTypeHandling ? JsonConvert.DeserializeObject<T>(json, _newtonsoftSettings) : JsonConvert.DeserializeObject<T>(json);
    }

    public static List<string> ValidateJson(JsonSchema jsonSchema, string jsonObject)
    {
        var json = JArray.Parse(jsonObject);

        bool valid = json.IsValid(jsonSchema, out var messages);

        return messages.ToList();
    }

    private static JsonSerializerSettings _newtonsoftSettings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.Auto,
        ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
    };

}
