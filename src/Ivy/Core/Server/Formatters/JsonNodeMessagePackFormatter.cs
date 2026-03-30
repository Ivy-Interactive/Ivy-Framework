using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Buffers;
using MessagePack;
using MessagePack.Formatters;

namespace Ivy.Core.Server.Formatters;

/// <summary>
/// A custom MessagePack formatter that serializes and deserializes System.Text.Json.Nodes.JsonNode objects.
/// This allows Ivy's differential state engine to utilize highly efficient binary bandwidth without changing its native dictionary-free architecture.
/// </summary>
public sealed class JsonNodeMessagePackFormatter : IMessagePackFormatter<JsonNode?>
{
    public void Serialize(ref MessagePackWriter writer, JsonNode? value, MessagePackSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNil();
            return;
        }

        if (value is JsonObject jsonObj)
        {
            writer.WriteMapHeader(jsonObj.Count);
            foreach (var kvp in jsonObj)
            {
                writer.Write(kvp.Key);
                Serialize(ref writer, kvp.Value, options);
            }
        }
        else if (value is JsonArray jsonArr)
        {
            writer.WriteArrayHeader(jsonArr.Count);
            foreach (var item in jsonArr)
            {
                Serialize(ref writer, item, options);
            }
        }
        else if (value is JsonValue jsonVal)
        {
            if (jsonVal.TryGetValue(out JsonElement element))
            {
                switch (element.ValueKind)
                {
                    case JsonValueKind.String:
                        writer.Write(element.GetString());
                        break;
                    case JsonValueKind.Number:
                        if (element.TryGetInt32(out int i32))
                            writer.Write(i32);
                        else if (element.TryGetInt64(out long i64))
                            writer.Write(i64);
                        else if (element.TryGetDouble(out double d))
                            writer.Write(d);
                        else
                            writer.Write(element.GetDouble());
                        break;
                    case JsonValueKind.True:
                        writer.Write(true);
                        break;
                    case JsonValueKind.False:
                        writer.Write(false);
                        break;
                    case JsonValueKind.Null:
                    case JsonValueKind.Undefined:
                        writer.WriteNil();
                        break;
                    case JsonValueKind.Object:
                    case JsonValueKind.Array:
                        var subNode = JsonNode.Parse(element.GetRawText());
                        Serialize(ref writer, subNode, options);
                        break;
                }
            }
            else if (jsonVal.TryGetValue(out string? s)) writer.Write(s);
            else if (jsonVal.TryGetValue(out int i)) writer.Write(i);
            else if (jsonVal.TryGetValue(out long l)) writer.Write(l);
            else if (jsonVal.TryGetValue(out double d)) writer.Write(d);
            else if (jsonVal.TryGetValue(out bool b)) writer.Write(b);
            else if (jsonVal.TryGetValue(out float f)) writer.Write(f);
            else if (jsonVal.TryGetValue(out decimal dec)) writer.Write((double)dec);
            else if (jsonVal.TryGetValue(out DateTime dt)) writer.Write(dt);
            else writer.Write(jsonVal.ToString());
        }
    }

    public JsonNode? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        var type = reader.NextMessagePackType;
        switch (type)
        {
            case MessagePackType.Map:
                var count = reader.ReadMapHeader();
                var obj = new JsonObject();
                for (int i = 0; i < count; i++)
                {
                    var key = reader.ReadString();
                    var value = Deserialize(ref reader, options);
                    if (key != null)
                    {
                        obj[key] = value;
                    }
                }
                return obj;

            case MessagePackType.Array:
                var arrCount = reader.ReadArrayHeader();
                var arr = new JsonArray();
                for (int i = 0; i < arrCount; i++)
                {
                    arr.Add(Deserialize(ref reader, options));
                }
                return arr;

            case MessagePackType.String:
                return JsonValue.Create(reader.ReadString());

            case MessagePackType.Integer:
                try
                {
                    return JsonValue.Create(reader.ReadInt64());
                }
                catch
                {
                    return JsonValue.Create(reader.ReadUInt64());
                }

            case MessagePackType.Float:
                return JsonValue.Create(reader.ReadDouble());

            case MessagePackType.Boolean:
                return JsonValue.Create(reader.ReadBoolean());

            case MessagePackType.Binary:
                return JsonValue.Create(reader.ReadBytes()?.ToArray() ?? Array.Empty<byte>());

            default:
                reader.Skip();
                return null;
        }
    }
}

public sealed class JsonObjectMessagePackFormatter : IMessagePackFormatter<JsonObject?>
{
    private static readonly JsonNodeMessagePackFormatter _base = new();
    public void Serialize(ref MessagePackWriter writer, JsonObject? value, MessagePackSerializerOptions options) => _base.Serialize(ref writer, value, options);
    public JsonObject? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => (JsonObject?)_base.Deserialize(ref reader, options);
}

public sealed class JsonArrayMessagePackFormatter : IMessagePackFormatter<JsonArray?>
{
    private static readonly JsonNodeMessagePackFormatter _base = new();
    public void Serialize(ref MessagePackWriter writer, JsonArray? value, MessagePackSerializerOptions options) => _base.Serialize(ref writer, value, options);
    public JsonArray? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => (JsonArray?)_base.Deserialize(ref reader, options);
}

public sealed class JsonValueMessagePackFormatter : IMessagePackFormatter<JsonValue?>
{
    private static readonly JsonNodeMessagePackFormatter _base = new();
    public void Serialize(ref MessagePackWriter writer, JsonValue? value, MessagePackSerializerOptions options) => _base.Serialize(ref writer, value, options);
    public JsonValue? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => (JsonValue?)_base.Deserialize(ref reader, options);
}
