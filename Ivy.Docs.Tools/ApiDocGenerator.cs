using System.Reflection;
using System.Text;
using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Widgets.Inputs;

namespace Ivy.Docs.Tools;

public static class ApiDocGenerator
{
    private static readonly Assembly IvyAssembly = typeof(IWidget).Assembly;

    private static readonly Dictionary<Type, string> TypeAliases = new()
    {
        [typeof(string)] = "string",
        [typeof(int)] = "int",
        [typeof(bool)] = "bool",
        [typeof(double)] = "double",
        [typeof(float)] = "float",
        [typeof(long)] = "long",
        [typeof(short)] = "short",
        [typeof(byte)] = "byte",
        [typeof(char)] = "char",
        [typeof(object)] = "object",
        [typeof(void)] = "void",
        [typeof(decimal)] = "decimal",
    };

    public static string GenerateApiDoc(string typeName, string? extensionTypesString, string? sourceUrl)
    {
        var type = GetTypeFromName(typeName);
        if (type == null)
        {
            return $"\n\n> WARNING: Unable to find type `{typeName}` for API documentation.\n\n";
        }

        var extensionTypes = ParseExtensionTypes(extensionTypesString);
        var sb = new StringBuilder();

        sb.AppendLine();
        sb.AppendLine("## API");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(sourceUrl))
        {
            var fileName = System.IO.Path.GetFileName(sourceUrl);
            sb.AppendLine($"[View Source: {fileName}]({sourceUrl})");
            sb.AppendLine();
        }

        var constructorsSection = GenerateConstructorsSection(type, extensionTypes);
        if (!string.IsNullOrEmpty(constructorsSection))
        {
            sb.AppendLine(constructorsSection);
        }

        var supportedTypesSection = GenerateSupportedTypesSection(type);
        if (!string.IsNullOrEmpty(supportedTypesSection))
        {
            sb.AppendLine(supportedTypesSection);
        }

        var propertiesSection = GeneratePropertiesSection(type, extensionTypes);
        if (!string.IsNullOrEmpty(propertiesSection))
        {
            sb.AppendLine(propertiesSection);
        }

        var eventsSection = GenerateEventsSection(type, extensionTypes);
        if (!string.IsNullOrEmpty(eventsSection))
        {
            sb.AppendLine(eventsSection);
        }

        return sb.ToString();
    }

    private static Type? GetTypeFromName(string typeName)
    {
        var type = IvyAssembly.GetType(typeName);
        if (type != null) return type;

        type = IvyAssembly.GetTypes()
            .FirstOrDefault(t => t.FullName?.StartsWith(typeName + "`") == true && t.IsGenericTypeDefinition);

        return type;
    }

    private static Type[] ParseExtensionTypes(string? extensionTypesString)
    {
        if (string.IsNullOrEmpty(extensionTypesString))
            return [];

        return extensionTypesString
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(name => IvyAssembly.GetType(name))
            .Where(t => t != null)
            .Cast<Type>()
            .ToArray();
    }

    private static object? TryToActivate(Type type)
    {
        try
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

            var primaryConstructor = type.GetConstructors()
                .FirstOrDefault(c => c.GetParameters().All(p => p.HasDefaultValue));
            if (primaryConstructor != null)
            {
                var parameters = primaryConstructor.GetParameters();
                var values = parameters.Select(p => p.DefaultValue).ToArray();
                return primaryConstructor.Invoke(values);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static string GenerateSupportedTypesSection(Type type)
    {
        var instance = TryToActivate(type);
        if (instance is not IAnyInput anyInput)
            return string.Empty;

        var allowedTypes = anyInput.SupportedStateTypes();
        if (allowedTypes.Length == 0)
            return string.Empty;

        var grouped = GroupAndPairSupportedTypes(allowedTypes);
        if (grouped.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        sb.AppendLine("### Supported Types");
        sb.AppendLine();
        sb.AppendLine("| Group | Type | Nullable |");
        sb.AppendLine("|-------|------|----------|");

        foreach (var (group, nonNullable, nullable) in grouped)
        {
            var nonNullableStr = nonNullable != null ? $"`{nonNullable}`" : "-";
            var nullableStr = nullable != null ? $"`{nullable}`" : "-";
            sb.AppendLine($"| {group} | {nonNullableStr} | {nullableStr} |");
        }
        sb.AppendLine();

        return sb.ToString();
    }

    private static List<(string Group, string? NonNullable, string? Nullable)> GroupAndPairSupportedTypes(Type[] types)
    {
        var groups = new List<(string Group, HashSet<Type> Types)>
        {
            ("Boolean", [typeof(bool)]),
            ("Numeric", [typeof(short), typeof(int), typeof(long), typeof(byte), typeof(float), typeof(double), typeof(decimal)]),
            ("Date/Time", [typeof(DateTime), typeof(DateOnly), typeof(DateTimeOffset), typeof(TimeOnly)]),
            ("Text", [typeof(string)]),
        };

        var typeSet = new HashSet<Type>(types);
        var result = new List<(string Group, string? NonNullable, string? Nullable)>();
        var handled = new HashSet<Type>();

        foreach (var (group, groupTypes) in groups)
        {
            foreach (var baseType in groupTypes)
            {
                Type? nullableType = null;
                if (baseType.IsValueType && Nullable.GetUnderlyingType(baseType) == null)
                {
                    nullableType = typeof(Nullable<>).MakeGenericType(baseType);
                }

                var hasNonNullable = typeSet.Contains(baseType);
                var hasNullable = nullableType != null && typeSet.Contains(nullableType);

                if (hasNonNullable || hasNullable)
                {
                    result.Add((group,
                        hasNonNullable ? GetTypeName(baseType) : null,
                        hasNullable ? GetTypeName(nullableType!) : null));
                    handled.Add(baseType);
                    if (nullableType != null) handled.Add(nullableType);
                }
            }
        }

        foreach (var t in typeSet)
        {
            if (handled.Contains(t)) continue;

            var isNullable = Nullable.GetUnderlyingType(t) != null;
            var baseType = Nullable.GetUnderlyingType(t) ?? t;
            var group = baseType.IsEnum ? "Enum" : "Other";

            if (result.Any(r => r.NonNullable == GetTypeName(baseType)))
                continue;

            var nullableType = baseType.IsValueType ? typeof(Nullable<>).MakeGenericType(baseType) : null;
            result.Add((group,
                typeSet.Contains(baseType) ? GetTypeName(baseType) : null,
                nullableType != null && typeSet.Contains(nullableType) ? GetTypeName(nullableType) : null));
        }

        return result;
    }

    private static string GenerateConstructorsSection(Type type, Type[] extensionTypes)
    {
        var sb = new StringBuilder();
        var signatures = new List<string>();

        foreach (var ctor in type.GetConstructors())
        {
            var sig = GetConstructorSignature(ctor);
            if (!string.IsNullOrEmpty(sig))
                signatures.Add(sig);
        }

        foreach (var extType in extensionTypes)
        {
            var toMethods = extType.GetMethods()
                .Where(m => m.Name.StartsWith("To") &&
                           m.IsStatic &&
                           m.IsPublic &&
                           m.GetParameters().FirstOrDefault()?.ParameterType == typeof(IAnyState));

            foreach (var method in toMethods)
            {
                var sig = GetMethodSignature(method);
                if (!string.IsNullOrEmpty(sig))
                    signatures.Add(sig);
            }
        }

        if (signatures.Count == 0)
            return string.Empty;

        sb.AppendLine("### Constructors");
        sb.AppendLine();
        sb.AppendLine("| Signature |");
        sb.AppendLine("|-----------|");
        foreach (var sig in signatures)
        {
            sb.AppendLine($"| `{sig}` |");
        }
        sb.AppendLine();

        return sb.ToString();
    }

    private static string GeneratePropertiesSection(Type type, Type[] extensionTypes)
    {
        var properties = type.GetProperties()
            .Where(p => p.GetCustomAttribute<PropAttribute>() != null)
            .Where(p => p.Name != "TestId")
            .OrderBy(p => p.Name)
            .ToList();

        if (properties.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        sb.AppendLine("### Properties");
        sb.AppendLine();
        sb.AppendLine("| Name | Type | Setters |");
        sb.AppendLine("|------|------|---------|");

        foreach (var prop in properties)
        {
            var propType = GetTypeName(prop.PropertyType);
            var setters = GetExtensionMethodsForProperty(prop, type, extensionTypes);
            sb.AppendLine($"| `{prop.Name}` | `{propType}` | {(string.IsNullOrEmpty(setters) ? "-" : $"`{setters}`")} |");
        }
        sb.AppendLine();

        return sb.ToString();
    }

    private static string GenerateEventsSection(Type type, Type[] extensionTypes)
    {
        var events = type.GetProperties()
            .Where(p => p.GetCustomAttribute<EventAttribute>() != null)
            .OrderBy(p => p.Name)
            .ToList();

        if (events.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        sb.AppendLine("### Events");
        sb.AppendLine();
        sb.AppendLine("| Name | Type | Handlers |");
        sb.AppendLine("|------|------|----------|");

        foreach (var evt in events)
        {
            var evtType = GetTypeName(evt.PropertyType);
            var handlers = GetExtensionMethodsForProperty(evt, type, extensionTypes);
            sb.AppendLine($"| `{evt.Name}` | `{evtType}` | {(string.IsNullOrEmpty(handlers) ? "-" : $"`{handlers}`")} |");
        }
        sb.AppendLine();

        return sb.ToString();
    }

    private static string GetConstructorSignature(ConstructorInfo ctor)
    {
        var type = ctor.DeclaringType!;
        var typeName = GetSimpleTypeName(type);
        var parameters = GetParameterList(ctor.GetParameters());
        return $"new {typeName}({parameters})";
    }

    private static string GetMethodSignature(MethodInfo method)
    {
        var parameters = GetParameterList(method.GetParameters());
        return $"{method.Name}({parameters})";
    }

    private static string GetParameterList(ParameterInfo[] parameters)
    {
        var parts = parameters.Select(p =>
        {
            var typeName = GetTypeName(p.ParameterType);
            var defaultValue = p.IsOptional ? $" = {FormatDefaultValue(p.DefaultValue)}" : "";
            return $"{typeName} {p.Name}{defaultValue}";
        });
        return string.Join(", ", parts);
    }

    private static string GetExtensionMethodsForProperty(PropertyInfo prop, Type baseType, Type[] extensionTypes)
    {
        var methods = new List<string>();

        foreach (var extType in extensionTypes)
        {
            var relatedMethods = extType.GetMethods()
                .Where(m => m.IsStatic && m.IsPublic)
                .Where(m => IsExtensionMethodForType(m, baseType))
                .Where(m =>
                    m.Name == prop.Name ||
                    (prop.Name.StartsWith("On") && m.Name == "Handle" + prop.Name[2..]))
                .Select(m => m.Name);

            methods.AddRange(relatedMethods);
        }

        return string.Join(", ", methods.Distinct());
    }

    private static bool IsExtensionMethodForType(MethodInfo method, Type baseType)
    {
        var firstParam = method.GetParameters().FirstOrDefault();
        if (firstParam == null) return false;

        var paramType = firstParam.ParameterType;

        if (paramType == baseType) return true;

        if (paramType.IsAssignableFrom(baseType)) return true;

        var current = baseType.BaseType;
        while (current != null && current != typeof(object))
        {
            if (current.IsGenericType &&
                current.GetGenericTypeDefinition() == typeof(WidgetBase<>) &&
                paramType.IsAssignableFrom(current))
            {
                return true;
            }
            current = current.BaseType;
        }

        return false;
    }

    private static string GetSimpleTypeName(Type type)
    {
        if (!type.IsGenericType)
            return type.Name;

        var baseName = type.Name[..type.Name.IndexOf('`')];
        var args = string.Join(", ", type.GetGenericArguments().Select(GetTypeName));
        return $"{baseName}<{args}>";
    }

    private static string GetTypeName(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type);
        if (underlying != null)
            return GetTypeName(underlying) + "?";

        if (TypeAliases.TryGetValue(type, out var alias))
            return alias;

        if (type.IsGenericType)
        {
            var baseName = type.Name[..type.Name.IndexOf('`')];
            var args = string.Join(", ", type.GetGenericArguments().Select(GetTypeName));
            return $"{baseName}<{args}>";
        }

        return type.Name;
    }

    private static string FormatDefaultValue(object? value)
    {
        if (value == null) return "null";
        if (value is bool b) return b ? "true" : "false";
        if (value is string s) return $"\"{s}\"";
        if (value is Enum e) return $"{e.GetType().Name}.{e}";
        return value.ToString() ?? "null";
    }
}

