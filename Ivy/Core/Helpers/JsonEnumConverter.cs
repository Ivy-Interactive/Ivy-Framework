using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ivy.Core.Helpers;

public class JsonEnumConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsEnum;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(EnumDescriptionConverterInner<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType)! ?? throw new InvalidOperationException();
    }

    private class EnumDescriptionConverterInner<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
    {
        public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? enumText = reader.GetString();
            if (enumText == null)
            {
                throw new JsonException($"Cannot convert null to {typeof(TEnum).Name}.");
            }

            foreach (var value in Enum.GetValues(typeof(TEnum)))
            {
                var description = GetEnumDescription((TEnum)value);
                if (string.Equals(enumText, description, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(enumText, value.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return (TEnum)value;
                }
            }

            throw new JsonException($"Invalid value '{enumText}' for enum {typeof(TEnum).Name}.");
        }

        public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }

        private string GetEnumDescription(TEnum value)
        {
            FieldInfo? fieldInfo = typeof(TEnum).GetField(value.ToString());
            if (fieldInfo != null)
            {
                var descriptionAttribute = fieldInfo.GetCustomAttribute<DescriptionAttribute>();
                if (descriptionAttribute != null)
                {
                    return descriptionAttribute.Description;
                }
            }

            return value.ToString();
        }
    }
}
