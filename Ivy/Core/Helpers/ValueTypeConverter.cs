using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ivy.Core.Helpers;

public class ValueTupleConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert is { IsValueType: true, IsGenericType: true } &&
        typeToConvert.FullName!.StartsWith("System.ValueTuple");

    public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options)
    {
        var converterType = typeof(ValueTupleConverter<>).MakeGenericType(type);
        return (JsonConverter)Activator.CreateInstance(converterType, options.PropertyNamingPolicy)!;
    }
}

public class ValueTupleConverter<T> : JsonConverter<T>
{
    private readonly FieldInfo[] _fields;
    private readonly Dictionary<string, FieldInfo> _fieldMap;

    public ValueTupleConverter(JsonNamingPolicy namingPolicy)
    {
        _fields = typeof(T)
            .GetFields(BindingFlags.Instance | BindingFlags.Public)
            .OrderBy(f => f.Name)
            .ToArray();

        _fieldMap = new Dictionary<string, FieldInfo>();
        foreach (var field in _fields)
        {
            string jsonName = namingPolicy?.ConvertName(field.Name) ?? field.Name;
            _fieldMap[jsonName] = field;
        }
    }

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        object[] values = new object[_fields.Length];
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;
            string propertyName = reader.GetString()!;
            reader.Read();
            if (_fieldMap.TryGetValue(propertyName, out var field))
            {
                int index = Array.IndexOf(_fields, field);
                values[index] = JsonSerializer.Deserialize(ref reader, field.FieldType, options)!;
            }
            else
            {
                reader.Skip();
            }
        }
        T tuple = (T)Activator.CreateInstance(typeof(T))!;
        for (int i = 0; i < _fields.Length; i++)
        {
            _fields[i].SetValueDirect(__makeref(tuple), values[i]);
        }
        return tuple;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (var field in _fields)
        {
            string jsonName = options.PropertyNamingPolicy?.ConvertName(field.Name) ?? field.Name;
            writer.WritePropertyName(jsonName);
            JsonSerializer.Serialize(writer, field.GetValue(value), field.FieldType, options);
        }
        writer.WriteEndObject();
    }
}