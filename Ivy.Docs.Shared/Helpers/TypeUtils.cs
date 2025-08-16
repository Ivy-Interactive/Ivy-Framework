using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Ivy.Core.Docs;
using Ivy.Docs.Shared.Apps.ApiReference.IvyShared;

namespace Ivy.Docs.Shared.Helpers;

public record PropRecord(string Name, object Type, object DefaultValue, object? Setters);
public record EventRecord(string Name, object Type, object? Setters);
public record SignatureRecord(object Signature);
public record SupportedTypeRecord(object Type);

public static class TypeUtils
{
    private static readonly Dictionary<Type, object> _simple = new()
    {
        { typeof(bool), "bool" },
        { typeof(byte), "byte" },
        { typeof(sbyte), "sbyte" },
        { typeof(char), "char" },
        { typeof(decimal), "decimal" },
        { typeof(double), "double" },
        { typeof(float), "float" },
        { typeof(int), "int" },
        { typeof(uint), "uint" },
        { typeof(long), "long" },
        { typeof(ulong), "ulong" },
        { typeof(object), "object" },
        { typeof(short), "short" },
        { typeof(ushort), "ushort" },
        { typeof(string), "string" },
        { typeof(void), "void" },
    };

    private static readonly Dictionary<Type, object> _apps = new()
    {
        { typeof(Align), new AlignApp(true)},
        { typeof(Colors), new ColorsApp(true)},
        { typeof(Icons), new IconsApp(true)},
        { typeof(Size), new SizeApp(true)},
        { typeof(Thickness), new ThicknessApp(true)},
    };

    private static object GetTypeName(Type type, bool isNullable)
    {
        if (Nullable.GetUnderlyingType(type) is { } underlying)
            return GetTypeDescription(underlying, true);

        if (_simple.TryGetValue(type, out var keyword))
            return isNullable ? $"{keyword}?" : keyword;

        if (type.IsGenericType)
        {
            var typeName = type.Name[..type.Name.IndexOf('`')];
            var genericArgs = string.Join(", ", type.GetGenericArguments().Select(arg => GetTypeName(arg, false)));
            return $"{typeName}<{genericArgs}>";
        }

        return isNullable && type.IsClass ? $"{type.Name}?" : type.Name;
    }

    private static object GetTypeDescription(Type type, bool isNullable)
    {
        if (Nullable.GetUnderlyingType(type) is { } underlying)
            return GetTypeDescription(underlying, true);

        if (_simple.TryGetValue(type, out var keyword))
            return isNullable ? $"{keyword}?" : keyword;

        if (_apps.TryGetValue(type, out var app))
        {
            var name = type.FullName.EatLeft("Ivy.").EatLeft("Shared.");
            return new Button(name + (isNullable ? "?" : "")).Inline()
                .WithSheet(() => app, width: Size.Units(150));
        }

        if (type.IsEnum)
        {
            return new Button(type.Name + (isNullable ? "?" : "")).Inline()
                .WithSheet(() => new EnumValuesView(type), width: Size.Units(150));
        }

        if (type.IsGenericType)
        {
            var typeName = type.Name[..type.Name.IndexOf('`')];
            var genericArgs = string.Join(", ", type.GetGenericArguments().Select(arg => GetTypeDescription(arg, false)));
            return $"{typeName}<{genericArgs}>";
        }

        return isNullable && type.IsClass ? $"{type.Name}?" : type.Name;
    }

    private static object GetPropertyTypeDescription(PropertyInfo prop)
    {
        var isNullable = IsNullableReference(prop);
        return GetTypeDescription(prop.PropertyType, isNullable);
    }

    internal static bool IsNullableReference(PropertyInfo prop)
    {
        var attr = prop.GetCustomAttributesData()
            .FirstOrDefault(a => a.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");

        if (attr?.ConstructorArguments.Count > 0)
        {
            var arg = attr.ConstructorArguments[0];

            if (arg.ArgumentType == typeof(byte) && (byte)arg.Value! == 2)
                return true;

            if (arg.ArgumentType == typeof(byte[]) &&
                arg.Value is IReadOnlyCollection<CustomAttributeTypedArgument> { Count: > 0 } items && (byte)items.First().Value! == 2)
                return true;
        }

        return false;
    }

    public static Type? GetTypeFromName(string typeName)
    {
        var assembly = typeof(IWidget).Assembly;
        var type = assembly.GetType(typeName) ?? assembly.GetTypes()
            .FirstOrDefault(t => t.FullName.StartsWith(typeName + "`") && t.IsGenericTypeDefinition);
        return type;
    }

    public static object? TryToActivate(Type type)
    {
        if (type.ContainsGenericParameters)
        {
            if (type.GetGenericArguments().Length == 1)
            {
                type = type.MakeGenericType(typeof(object));
            }
            else
            {
                return null;
            }
        }
        var constructor = type.GetConstructor(Type.EmptyTypes);
        if (constructor != null)
        {
            return constructor.Invoke([]);
        }
        var primaryConstructor = type.GetConstructors().FirstOrDefault(c => c.GetParameters().All(p => p.HasDefaultValue));
        if (primaryConstructor != null)
        {
            var parameters = primaryConstructor.GetParameters();
            var values = parameters.Select(p => p.DefaultValue).ToArray();
            return primaryConstructor.Invoke(values);
        }
        return null;
    }

    public static SupportedTypeRecord GetSupportedTypeRecord(Type type)
    {
        return new SupportedTypeRecord(GetTypeDescription(type, false));
    }

    public static PropRecord GetPropRecord(PropertyInfo prop, object? defaultValueProvider, Type baseType, Type[] extensionsTypes)
    {
        object GetDefaultValue()
        {
            try
            {
                if (defaultValueProvider == null) return null;
                var defaultValue = prop.GetValue(defaultValueProvider);
                var literal = CSharpLiteralGenerator.ToCSharpLiteral(defaultValue);
                literal = literal?.EatLeft("Ivy.").EatLeft("Shared.");
                return literal != null ? new Code(literal, Languages.Csharp).ShowCopyButton(false).ShowBorder(false) : defaultValue.ToString();
            }
            catch (Exception)
            {
                return null;
            }
        }

        // ReSharper disable once LocalFunctionHidesMethod
        object GetExtensionMethods()
        {
            if (extensionsTypes.Length == 0) return null;
            var extensions = TypeUtils.GetExtensionMethods(prop, baseType, extensionsTypes);
            if (!string.IsNullOrEmpty(extensions))
            {
                return new Code(extensions, Languages.Csharp).ShowCopyButton(false).ShowBorder(false);
            }
            return null;
        }

        return new PropRecord(prop.Name, GetPropertyTypeDescription(prop), GetDefaultValue(), GetExtensionMethods());
    }

    public static EventRecord GetEventRecord(PropertyInfo prop, Type baseType, Type[] extensionsTypes)
    {
        // ReSharper disable once LocalFunctionHidesMethod
        object GetExtensionMethods()
        {
            if (extensionsTypes.Length == 0) return null;
            var extensions = TypeUtils.GetExtensionMethods(prop, baseType, extensionsTypes);
            if (!string.IsNullOrEmpty(extensions))
            {
                return new Code(extensions, Languages.Csharp).ShowCopyButton(false).ShowBorder(false);
            }
            return null;
        }

        return new EventRecord(prop.Name, GetPropertyTypeDescription(prop), GetExtensionMethods());
    }

    internal static string GetExtensionMethods(PropertyInfo propertyInfo, Type baseType, Type[] extensionsTypes)
    {
        var sb = new StringBuilder();

        //In extensionsTypes find all public static methods:
        //1) that are extension methods for baseType
        //2) and either have the same name as the propertyInfo or have an attribute of type RelatedToAttribute with the value of propertyInfo.Name
        var methods = extensionsTypes
            .SelectMany(t => t.GetMethods())
            .Where(m => m is { IsStatic: true, IsPublic: true } && m.GetCustomAttribute<ExtensionAttribute>() != null)
            .Where(m => m.GetParameters().First().ParameterType == baseType || m.GetParameters().First().ParameterType.IsWidgetBaseType())
            .Where(m =>
                m.Name == propertyInfo.Name ||
                (propertyInfo.Name.StartsWith("On") && m.Name == ("Handle" + propertyInfo.Name[2..])) ||
                m.GetCustomAttribute<RelatedToAttribute>()?.PropertyName == propertyInfo.Name);

        foreach (var method in methods)
        {
            var parameters = method.GetParameters().Skip(1);
            var paramSignatures = parameters.Select(p => $"{GetCSharpTypeName(p.ParameterType)} {p.Name}{(p.IsOptional ? " = " + CSharpLiteralGenerator.ToCSharpLiteral(p.DefaultValue).EatLeft("Ivy.Shared.") : "")}");
            sb.AppendLine($"{method.Name}({string.Join(", ", paramSignatures)})");
        }

        return sb.ToString().Trim();
    }

    private static bool IsWidgetBaseType(this Type type)
    {
        Type? currentType = type.BaseType;

        while (currentType != null && currentType != typeof(object))
        {
            // Check if the current type is a generic type
            if (currentType.IsGenericType)
            {
                // Get the generic type definition (without the type arguments)
                Type genericTypeDef = currentType.GetGenericTypeDefinition();

                // Check if it's WidgetBase<>
                if (genericTypeDef == typeof(Ivy.WidgetBase<>))
                {
                    return true;
                }
            }

            // Move up the inheritance chain
            currentType = currentType.BaseType;
        }

        return false;
    }

    public static SignatureRecord GetSignatureRecord(ConstructorInfo constructor)
    {
        return new SignatureRecord(new Code(GetCSharpSignature(constructor)).ShowCopyButton(false).ShowBorder(false));
    }

    public static SignatureRecord GetSignatureRecord(MethodInfo constructor)
    {
        return new SignatureRecord(new Code(GetCSharpSignature(constructor)).ShowCopyButton(false).ShowBorder(false));
    }

    private static string GetCSharpSignature(ConstructorInfo constructor)
    {
        var type = constructor.DeclaringType;
        var typeName = type.Name;
        if (type.IsGenericType)
        {
            var genericArgs = string.Join(", ", type.GetGenericArguments().Select(GetCSharpTypeName));
            typeName = $"{type.Name[..type.Name.IndexOf('`')]}<{genericArgs}>";
        }

        var parameters = constructor.GetParameters();
        var paramSignatures = parameters.Select(p => $"{GetCSharpTypeName(p.ParameterType)} {p.Name}{(p.IsOptional ? " = " + CSharpLiteralGenerator.ToCSharpLiteral(p.DefaultValue).EatLeft("Ivy.Shared.") : "")}");
        return $"new {typeName}({string.Join(", ", paramSignatures)})";
    }

    private static string GetCSharpSignature(MethodInfo method)
    {
        var parameters = method.GetParameters();
        var paramSignatures = parameters.Select(p => $"{GetCSharpTypeName(p.ParameterType)} {p.Name}{(p.IsOptional ? " = " + CSharpLiteralGenerator.ToCSharpLiteral(p.DefaultValue).EatLeft("Ivy.Shared.") : "")}");
        return $"{method.Name}({string.Join(", ", paramSignatures)})";
    }

    private static string GetCSharpTypeName(Type type)
    {
        if (type == typeof(string)) return "string";
        if (type == typeof(int)) return "int";
        if (type == typeof(bool)) return "bool";
        if (type == typeof(double)) return "double";
        if (type == typeof(float)) return "float";
        if (type == typeof(long)) return "long";
        if (type == typeof(short)) return "short";
        if (type == typeof(byte)) return "byte";
        if (type == typeof(char)) return "char";
        if (type == typeof(object)) return "object";
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            return GetCSharpTypeName(type.GetGenericArguments()[0]) + "?";

        if (type.IsGenericType)
        {
            var typeName = type.Name[..type.Name.IndexOf('`')];
            var genericArgs = string.Join(", ", type.GetGenericArguments().Select(GetCSharpTypeName));
            return $"{typeName}<{genericArgs}>";
        }

        return type.Name;
    }

    public static List<(string Group, object NonNullable, object Nullable)> GroupAndPairSupportedTypes(IEnumerable<Type> types)
    {
        // Define groups and their type sets
        var groups = new List<(string Group, HashSet<Type> Types)>
        {
            ("Boolean", new HashSet<Type> { typeof(bool) }),
            ("Numeric", new HashSet<Type> { typeof(short), typeof(int), typeof(long), typeof(byte), typeof(float), typeof(double), typeof(decimal) }),
            ("Date/Time", new HashSet<Type> { typeof(DateTime), typeof(DateOnly), typeof(DateTimeOffset), typeof(TimeOnly) }),
            ("Color", new HashSet<Type> { typeof(Colors) }),
            ("Text", new HashSet<Type> { typeof(string) }),
        };

        // Flatten all types to base/non-nullable and nullable
        var typeSet = new HashSet<Type>(types);
        var result = new List<(string Group, object NonNullable, object Nullable)>();
        var handled = new HashSet<Type>();

        foreach (var (group, groupTypes) in groups)
        {
            foreach (var baseType in groupTypes)
            {
                var nonNullable = baseType;
                Type? nullable = null;
                if (baseType.IsValueType && Nullable.GetUnderlyingType(baseType) == null)
                {
                    nullable = typeof(Nullable<>).MakeGenericType(baseType);
                }
                var hasNonNullable = typeSet.Contains(nonNullable);
                var hasNullable = nullable != null && typeSet.Contains(nullable);
                if (hasNonNullable || hasNullable)
                {
                    result.Add((group,
                        hasNonNullable ? GetTypeDescription(nonNullable, false) : null!,
                        hasNullable ? GetTypeDescription(nullable!, true) : null!));
                    handled.Add(nonNullable);
                    if (nullable != null) handled.Add(nullable);
                }
            }
        }
        // Handle enums and other types not in the above groups
        foreach (var t in typeSet)
        {
            if (handled.Contains(t)) continue;
            var isNullable = Nullable.GetUnderlyingType(t) != null;
            var baseType = Nullable.GetUnderlyingType(t) ?? t;
            var group = baseType.IsEnum ? "Enum" : "Other";
            // Pair nullable/non-nullable enums/other
            if (!result.Any(r => r.NonNullable?.ToString() == GetTypeDescription(baseType, false).ToString()))
            {
                var nullableType = typeof(Nullable<>).MakeGenericType(baseType);
                result.Add((group,
                    typeSet.Contains(baseType) ? GetTypeDescription(baseType, false) : null!,
                    typeSet.Contains(nullableType) ? GetTypeDescription(nullableType, true) : null!));
            }
        }
        return result;
    }
}
