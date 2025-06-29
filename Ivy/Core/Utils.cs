using System.Collections;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Ivy.Core;

public static class Utils
{
    public static int StableHash(string text)
    {
        unchecked
        {
            int hash = 23;
            foreach (char c in text)
                hash = (hash * 31) + c;
            return hash;
        }
    }

    public static object? ConvertJsonNode(JsonNode? jsonNode, Type valueType)
    {
        if (jsonNode is null) return null;
        // Get underlying type to handle nullable types properly with Convert.ChangeType
        var t = Nullable.GetUnderlyingType(valueType) ?? valueType;

        if (valueType == typeof(object))
        {
            return BestGuessConvert(ConvertJsonNodeToObject(jsonNode), valueType);
        }

        if (valueType.IsCollectionType() && valueType.GetCollectionTypeParameter() is { } itemType)
        {
            if (jsonNode is JsonArray jsonArray)
            {
                var items = jsonArray.Select(e => ConvertJsonNode(e, itemType)).ToArray();
                if (valueType.IsArray)
                {
                    // Create array of the correct type
                    var array = Array.CreateInstance(itemType, items.Length);
                    for (int i = 0; i < items.Length; i++)
                    {
                        array.SetValue(items[i], i);
                    }
                    return array;
                }

                if (valueType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var list = (IList)Activator.CreateInstance(valueType)!;
                    foreach (var item in items)
                        list.Add(item);
                    return list;
                }

                var listType = typeof(List<>).MakeGenericType(itemType);
                var genericList = (IList)Activator.CreateInstance(listType)!;
                foreach (var item in items)
                    genericList.Add(item);
                return genericList;
            }
        }

        if (t.IsEnum && jsonNode is JsonValue enumVal && enumVal.TryGetValue(out string? enumStr))
            return Enum.Parse(t, enumStr, true);

        if (t == typeof(bool) && jsonNode is JsonValue boolVal)
        {
            return boolVal switch
            {
                _ when boolVal.TryGetValue(out bool b) => b,
                _ when boolVal.TryGetValue(out int i) => i != 0,
                _ when boolVal.TryGetValue(out long l) => l != 0,
                _ when boolVal.TryGetValue(out double d) => d != 0,
                _ when boolVal.TryGetValue(out string? s) =>
                    bool.TryParse(s, out var parsed) ? parsed :
                    double.TryParse(s, out var num) && num != 0,
                _ => false
            };
        }

        if (t.IsNumeric() && jsonNode is JsonValue boolVal1 && boolVal1.TryGetValue(out bool _))
        {
            var convertedValue = Convert.ChangeType(boolVal1.GetValue<bool>() ? 1 : 0, t);
            return convertedValue;
        }

        if (IsNumericType(t) && jsonNode is JsonValue numVal)
        {
            return numVal switch
            {
                _ when numVal.TryGetValue(out string? str) => Convert.ChangeType(str, t),
                _ when numVal.TryGetValue(out double dbl) => Convert.ChangeType(dbl, t),
                _ when numVal.TryGetValue(out long lng) => Convert.ChangeType(lng, t),
                _ when numVal.TryGetValue(out int intVal) => Convert.ChangeType(intVal, t),
                _ => null
            };
        }

        //todo: maybe make this more generic
        if (IsValueTupleOfTwo(t) && jsonNode is JsonObject obj)
        {
            var (item1Type, item2Type) = GetValueTupleTypes(t);
            var item1 = obj.ContainsKey("item1") ? ConvertJsonNode(obj["item1"]!, item1Type) : null;
            var item2 = obj.ContainsKey("item2") ? ConvertJsonNode(obj["item2"]!, item2Type) : null;
            return Activator.CreateInstance(t, item1, item2);
        }

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
        };

        return jsonNode.Deserialize(valueType, options);
    }

    // public class Base64ByteArrayConverter : JsonConverter<byte[]>
    // {
    //     public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    //     {
    //         var base64 = reader.GetString();
    //         if (string.IsNullOrEmpty(base64)) 
    //             return [];
    //         return Convert.FromBase64String(base64);
    //     }
    //
    //     public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
    //     {
    //         if (value == null || value.Length == 0)
    //         {
    //             writer.WriteStringValue(string.Empty);
    //             return;
    //         }
    //         writer.WriteStringValue(Convert.ToBase64String(value));
    //     }
    // }
    //
    private static bool IsNumericType(Type t)
    {
        if (t.IsPrimitive && t != typeof(bool) && t != typeof(char) &&
            t != typeof(IntPtr) && t != typeof(UIntPtr)) return true;
        return t == typeof(decimal);
    }

    private static bool IsValueTupleOfTwo(Type t) =>
        t is { IsValueType: true, IsGenericType: true } && t.GetGenericTypeDefinition() == typeof(ValueTuple<,>);

    private static (Type, Type) GetValueTupleTypes(Type t)
    {
        var args = t.GetGenericArguments();
        return (args[0], args[1]);
    }

    public static object? BestGuessConvert(object? input, Type targetType)
    {
        if (input == null) return null;
        if (targetType.IsInstanceOfType(input)) return input;

        var underlyingType = Nullable.GetUnderlyingType(targetType);
        if (underlyingType != null)
            return BestGuessConvert(input, underlyingType);

        if (targetType == typeof(DateTime) && input is string dateString)
        {
            if (DateTime.TryParse(dateString, out var dt)) return dt;
            return null;
        }

        if (targetType == typeof(Guid) && input is string guidString)
        {
            if (Guid.TryParse(guidString, out var guid)) return guid;
            return null;
        }

        if (targetType.IsEnum && input is string enumString)
        {
            return Enum.Parse(targetType, enumString);
        }

        // Handle dictionary to tuple conversion
        if (IsTupleType(targetType) && input is IDictionary dictionary)
        {
            var tupleTypes = targetType.GetGenericArguments();
            var values = Enumerable.Range(1, tupleTypes.Length)
                .Select(i => new { Index = i - 1, Key = "item" + i })
                .Select(x => dictionary.Contains(x.Key)
                    ? BestGuessConvert(dictionary[x.Key], tupleTypes[x.Index])
                    : null)
                .ToArray();

            if (values.Any(v => v == null))
                return null;

            return Activator.CreateInstance(targetType, values);
        }

        if (input is IConvertible)
        {
            try
            {
                return Convert.ChangeType(input, targetType);
            }
            catch
            {
                return null;
            }
        }

        return null;
    }

    private static bool IsTupleType(Type type)
    {
        return type.IsGenericType && type.FullName?.StartsWith("System.ValueTuple") == true;
    }

    public static string PascalCaseToCamelCase(string titleCase)
    {
        if (string.IsNullOrWhiteSpace(titleCase))
            return string.Empty;

        string camelCase = char.ToLower(titleCase[0]) + titleCase[1..];

        return camelCase;
    }

    public static object?[] ConvertJsonArrayToObjectArray(JsonArray? jsonArray)
    {
        if (jsonArray == null)
            return [];

        return jsonArray.Select(e => e != null ? ConvertJsonNodeToObject(e) : null)
            .ToArray();
    }

    private static object? ConvertJsonNodeToObject(JsonNode? node)
    {
        if (node is null)
            return null;

        return node switch
        {
            JsonValue jv => jv.TryGetValue<int>(out var intValue)
                ? (object)intValue
                : jv.TryGetValue<double>(out var doubleValue)
                    ? doubleValue
                    : jv.TryGetValue<bool>(out var boolValue)
                        ? boolValue
                        : jv.TryGetValue<string>(out var stringValue)
                            ? stringValue
                            : throw new NotSupportedException(),
            JsonObject jo => jo.ToDictionary(kv => kv.Key, kv => ConvertJsonNodeToObject(kv.Value)),
            JsonArray ja => ja.Select(ConvertJsonNodeToObject).ToList(),
            _ => throw new NotSupportedException()
        };
    }

    public static Dictionary<string, object> ToDictionary(this JsonElement element)
    {
        return element.EnumerateObject()
            .ToDictionary(prop => prop.Name, prop => ConvertElement(prop.Value));
    }

    private static object ConvertElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => element.ToDictionary(),
            JsonValueKind.Array => element.EnumerateArray().Select(ConvertElement).ToList(),
            JsonValueKind.String => element.GetString()!,
            JsonValueKind.Number => element.TryGetInt64(out long l) ? l : element.GetDouble(),
            JsonValueKind.True or JsonValueKind.False => element.GetBoolean(),
            JsonValueKind.Null => null!,
            _ => element.GetRawText()
        };
    }

    public static string CleanGenericNotation(string typeName)
    {
        // Match pattern: any characters followed by a backtick and numbers
        return System.Text.RegularExpressions.Regex.Replace(typeName, @"`[\d]+", string.Empty);
    }
}