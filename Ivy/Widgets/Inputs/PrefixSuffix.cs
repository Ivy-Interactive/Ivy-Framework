using System.Text.Json;
using System.Text.Json.Serialization;
using Ivy.Shared;

namespace Ivy.Widgets.Inputs;

public abstract record PrefixSuffix
{
    private PrefixSuffix() { } // Prevent external inheritance

    public sealed record Text(string Value) : PrefixSuffix;

    public sealed record Icon(Icons Value) : PrefixSuffix;
}

internal class PrefixSuffixJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(PrefixSuffix).IsAssignableFrom(typeToConvert);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return new PrefixSuffixJsonConverter();
    }
}

internal class PrefixSuffixJsonConverter : JsonConverter<PrefixSuffix>
{
    public override PrefixSuffix? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty("type", out var typeElement) || !root.TryGetProperty("value", out var valueElement))
        {
            return null;
        }

        var type = typeElement.GetString();

        return type switch
        {
            "text" => new PrefixSuffix.Text(valueElement.GetString() ?? string.Empty),
            "icon" => Enum.TryParse<Icons>(valueElement.GetString(), out var iconValue) ? new PrefixSuffix.Icon(iconValue) : null,
            _ => null
        };
    }

    public override void Write(Utf8JsonWriter writer, PrefixSuffix value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        switch (value)
        {
            case PrefixSuffix.Text text:
                writer.WriteString("type", "text");
                writer.WriteString("value", text.Value);
                break;
            case PrefixSuffix.Icon icon:
                writer.WriteString("type", "icon");
                writer.WriteString("value", icon.Value.ToString());
                break;
        }

        writer.WriteEndObject();
    }
}