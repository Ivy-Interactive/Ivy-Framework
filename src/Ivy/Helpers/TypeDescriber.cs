using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Ivy;

public static class TypeDescriber
{
    public static string Describe(Type type)
    {
        var sb = new StringBuilder();
        AppendType(sb, type);
        return sb.ToString().TrimEnd();
    }

    public static string Describe(IEnumerable<Type> types, string? header = null)
    {
        var sb = new StringBuilder();
        if (header != null)
        {
            sb.AppendLine($"## {header}");
            sb.AppendLine();
        }

        var first = true;
        foreach (var type in types)
        {
            if (!first) sb.AppendLine();
            AppendType(sb, type);
            first = false;
        }

        return sb.ToString().TrimEnd();
    }

    private static void AppendType(StringBuilder sb, Type type)
    {
        var tableAttr = type.GetCustomAttribute<TableAttribute>();
        var tableSuffix = tableAttr != null ? $" (Table: {tableAttr.Name})" : "";
        sb.AppendLine($"{type.Name}{tableSuffix}");

        var nullabilityContext = new NullabilityInfoContext();
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Order: PK first, then FK/scalar, then navigation
        var ordered = properties
            .OrderBy(p => p.GetCustomAttribute<KeyAttribute>() == null ? 1 : 0)
            .ThenBy(p => IsNavigationProperty(p) ? 1 : 0)
            .ToList();

        foreach (var prop in ordered)
        {
            sb.AppendLine($"  {DescribeProperty(prop, nullabilityContext)}");
        }
    }

    private static string DescribeProperty(PropertyInfo prop, NullabilityInfoContext nullabilityContext)
    {
        var propType = prop.PropertyType;

        if (IsCollectionNavigation(propType))
        {
            var elementType = TypeHelper.GetCollectionTypeParameter(propType);
            var typeName = elementType != null ? $"Collection<{elementType.Name}>" : "Collection";
            return $"{prop.Name}: {typeName}";
        }

        var isNullableRef = propType.IsClass
            && nullabilityContext.Create(prop).WriteState == NullabilityState.Nullable;
        var friendlyName = GetFriendlyTypeName(propType) + (isNullableRef ? "?" : "");
        var annotations = GetAnnotations(prop);

        if (IsNavigationProperty(prop))
        {
            annotations.Add("Nav");
        }

        var annotationStr = annotations.Count > 0 ? $" [{string.Join(", ", annotations)}]" : "";
        return $"{prop.Name}: {friendlyName}{annotationStr}";
    }

    private static List<string> GetAnnotations(PropertyInfo prop)
    {
        var tags = new List<string>();

        if (prop.GetCustomAttribute<KeyAttribute>() != null)
            tags.Add("PK");

        var dbGenAttr = prop.GetCustomAttribute<DatabaseGeneratedAttribute>();
        if (dbGenAttr?.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity)
            tags.Add("Identity");

        if (prop.GetCustomAttribute<RequiredAttribute>() != null)
            tags.Add("Required");

        var fkAttr = prop.GetCustomAttribute<ForeignKeyAttribute>();
        if (fkAttr != null)
        {
            // ForeignKey on a navigation property: the Name points to the FK property,
            // and the target type is the property's own type.
            // ForeignKey on a scalar property: the Name points to the navigation property.
            if (IsNavigationProperty(prop))
            {
                tags.Add($"FK -> {prop.PropertyType.Name}");
            }
            else
            {
                // Resolve the navigation property to get the target type
                var navProp = prop.DeclaringType?.GetProperty(fkAttr.Name);
                if (navProp != null)
                    tags.Add($"FK -> {navProp.PropertyType.Name}");
                else
                    tags.Add("FK");
            }
        }

        var maxLengthAttr = prop.GetCustomAttribute<MaxLengthAttribute>();
        if (maxLengthAttr != null)
            tags.Add($"MaxLength({maxLengthAttr.Length})");

        var stringLengthAttr = prop.GetCustomAttribute<StringLengthAttribute>();
        if (stringLengthAttr != null && maxLengthAttr == null)
            tags.Add($"MaxLength({stringLengthAttr.MaximumLength})");

        return tags;
    }

    private static bool IsNavigationProperty(PropertyInfo prop)
    {
        var type = prop.PropertyType;
        if (IsCollectionNavigation(type)) return true;
        return !TypeHelper.IsSimpleType(type) && type != typeof(string) && type.IsClass;
    }

    private static bool IsCollectionNavigation(Type type)
    {
        if (type == typeof(string)) return false;
        return TypeHelper.IsCollectionType(type);
    }

    private static string GetFriendlyTypeName(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type);
        if (underlying != null)
            return GetFriendlyTypeName(underlying) + "?";

        return Type.GetTypeCode(type) switch
        {
            TypeCode.Boolean => "bool",
            TypeCode.Byte => "byte",
            TypeCode.Char => "char",
            TypeCode.DateTime => "DateTime",
            TypeCode.Decimal => "decimal",
            TypeCode.Double => "double",
            TypeCode.Int16 => "short",
            TypeCode.Int32 => "int",
            TypeCode.Int64 => "long",
            TypeCode.SByte => "sbyte",
            TypeCode.Single => "float",
            TypeCode.String => "string",
            TypeCode.UInt16 => "ushort",
            TypeCode.UInt32 => "uint",
            TypeCode.UInt64 => "ulong",
            _ => type.Name
        };
    }
}
