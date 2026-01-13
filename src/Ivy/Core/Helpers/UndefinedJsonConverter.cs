using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ivy.Core.Helpers;

public class UndefinedJsonConverter<T> : JsonConverter<T?>
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Handle the deserialization of 'undefined' if required, but it usually isn't present in JSON inputs.
        if (reader.TokenType == JsonTokenType.Null)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(ref reader, options);
    }

    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteRawValue("undefined");
        }
        else
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}