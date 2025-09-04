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