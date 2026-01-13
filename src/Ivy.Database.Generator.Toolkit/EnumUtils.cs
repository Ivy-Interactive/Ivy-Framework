using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.Serialization;

namespace Ivy.Database.Generator.Toolkit;

public interface IEnumEntity<TEnum> where TEnum : Enum
{
    int Id { get; set; }
    string DescriptionText { get; set; }
}

public static class EnumEntity
{
    public static TEntity[] Seed<TEntity, TEnum>(IEnumerable<EnumMetadataValue<TEnum>> values)
        where TEntity : class, IEnumEntity<TEnum>, new()
        where TEnum : Enum
    {
        var id = typeof(TEntity).GetProperty("Id");
        if (id == null)
            throw new ArgumentException($"TEntity of type '{typeof(TEntity)}' property 'Id' does not exist", "TEntity");

        return values.Select(value => new TEntity
        {
            Id = value.ValueAsInt32,
            DescriptionText = value.Name
        }).ToArray();
    }
}

public sealed class EnumMetadataValue<TEnum>(
    FieldInfo f,
    object value,
    string? name = null,
    string? description = null,
    int? order = null,
    Type? resourceType = null)
{
    public readonly object _value = value;

    private string? _valueAsString;

    public TEnum Value => (TEnum)_value;

    public int ValueAsInt32 => (int)_value;

    public string ValueAsString
    {
        get
        {
            if (_valueAsString == null) _valueAsString = GetCustomAttribute<EnumMemberAttribute>()?.Value ?? string.Empty;

            return _valueAsString;
        }
    }

    public string Name { get; } = name ?? f.Name.Replace('_', '.'); // fallback to something user friendlier...

    public string? Description { get; } = description;

    public int? Order { get; } = order;

    public Type? ResourceType { get; } = resourceType;

    public T? GetCustomAttribute<T>()
        where T : Attribute
    {
        return f.GetCustomAttribute<T>();
    }

    public IEnumerable<Attribute> GetCustomAttributes()
    {
        return f.GetCustomAttributes();
    }

    public override string ToString()
    {
        return Name;
    }
}

public static class EnumMetadata<TEnum> where TEnum : notnull
{
    private static readonly Dictionary<TEnum, EnumMetadataValue<TEnum>> _map = null!;

    private static ReadOnlyCollection<EnumMetadataValue<TEnum>>? _values;

    static EnumMetadata()
    {
        var enumType = typeof(TEnum);
        if (!enumType.IsEnum) throw new ArgumentException($"{enumType} is not a enum type.", "TEnum");

        var underlyingType = Enum.GetUnderlyingType(enumType);
        if (underlyingType != typeof(int))
            throw new ArgumentException($"{enumType} underlying type is not int.", "TEnum");

        var map = new Dictionary<TEnum, EnumMetadataValue<TEnum>>();

        foreach (var f in enumType.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (!f.IsLiteral) continue;

            var value = f.GetValue(null);

            string? name = null;
            Type? resourceType = null;
            string? description = null;
            int? order = null;

            var display = f.GetCustomAttribute<DisplayAttribute>();
            if (display != null)
            {
                name = display.Name;
                description = display.Description;
                order = display.GetOrder();
                resourceType = display.ResourceType;
            }

            var metadata = new EnumMetadataValue<TEnum>(f, value ?? throw new ArgumentNullException(nameof(value)), name, description,
                order, resourceType);
            map.Add(metadata.Value, metadata);
        }

        _map = map;
    }

    public static ReadOnlyCollection<EnumMetadataValue<TEnum>> Values
    {
        get
        {
            if (_values == null)
                _values = _map.Values.OrderBy(x => x.Order).ThenBy(x => x.ValueAsInt32).ToList().AsReadOnly();

            return _values;
        }
    }

    public static EnumMetadataValue<TEnum> Value(TEnum key)
    {
        EnumMetadataValue<TEnum>? metadata;
        if (_map.TryGetValue(key, out metadata)) return metadata;

        return null!; // undefined
    }
}

public static class EnumMetadata
{
    public static EnumMetadataValue<TEnum>? Value<TEnum>(TEnum key) where TEnum : notnull
    {
        return EnumMetadata<TEnum>.Value(key);
    }

    public static string? ToString<TEnum>(TEnum key) where TEnum : notnull
    {
        return Value(key)?.ToString();
    }

    public static string Serialize<TEnum>(TEnum value)
        where TEnum : struct, IComparable, IFormattable, IConvertible
    {
        var metadata = EnumMetadata<TEnum>.Value(value);
        if (metadata == null) throw new InvalidOperationException("cannot serialize enum value");

        var valueString = EnumMetadata<TEnum>.Value(value).ValueAsString;
        if (valueString == null)
            throw new InvalidOperationException(
                "cannot serialize enum value, missing [EnumMember(Value = ...)] attribute mapping");

        return valueString;
    }

    public static TEnum Deserialize<TEnum>(string s)
        where TEnum : struct, IComparable, IFormattable, IConvertible
    {
        var metadata = EnumMetadata<TEnum>.Values.FirstOrDefault(x => x.ValueAsString == s);
        if (metadata == null) throw new InvalidOperationException("cannot deserialize enum value");

        return metadata.Value;
    }
}