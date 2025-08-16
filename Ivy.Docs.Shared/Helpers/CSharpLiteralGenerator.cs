using System.Collections;
using System.Globalization;

namespace Ivy.Docs.Shared.Helpers;

public static class CSharpLiteralGenerator
{
    public static string? ToCSharpLiteral(object? obj)
    {
        return ToCSharpLiteral(obj, []);
    }

    private static string? ToCSharpLiteral(object? obj, HashSet<object> visited)
    {
        if (obj == null) return "null";

        if (!visited.Add(obj)) return "/* cyclic reference */ null";

        switch (obj)
        {
            case string s: return $"\"{s.Replace("\"", "\\\"")}\"";
            case char c: return $"'{c}'";
            case bool b: return b ? "true" : "false";
            case Enum e: return $"{e.GetType().FullName}.{e}";
            case byte or sbyte or short or ushort or int or uint or long or ulong:
                return Convert.ToString(obj, CultureInfo.InvariantCulture);
            case float f: return f.ToString("R", CultureInfo.InvariantCulture) + "f";
            case double d: return d.ToString("R", CultureInfo.InvariantCulture);
            case decimal m: return m.ToString(CultureInfo.InvariantCulture) + "m";
            case DateTime dt: return $"DateTime.Parse(\"{dt:O}\")";
            case IEnumerable list when obj is not string:
                var items = list.Cast<object>().Select(x => ToCSharpLiteral(x, visited));
                var type = obj.GetType();
                if (type.IsArray)
                    return $"new {type.GetElementType().FullName}[] {{ {string.Join(", ", items)} }}";
                return $"new List<{GetFriendlyTypeName(type.GenericTypeArguments[0])}> {{ {string.Join(", ", items)} }}";
        }

        return null;

        // var typeObj = obj.GetType();
        // var props = typeObj
        //     .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        //     .Where(p => p is { CanRead: true, GetMethod.IsPublic: true })
        //     .ToList();
        //
        // var initializers = props
        //     .Select(p => $"{p.Name} = {ToCSharpLiteral(p.GetValue(obj), visited)}");
        //
        // return $"new {GetFriendlyTypeName(typeObj)} {{ {string.Join(", ", initializers)} }}";
    }

    static string GetFriendlyTypeName(Type type)
    {
        if (!type.IsGenericType) return type.FullName;

        var genericTypeName = type.GetGenericTypeDefinition().FullName;
        var genericArgs = string.Join(", ", type.GetGenericArguments().Select(GetFriendlyTypeName));
        return $"{genericTypeName[..genericTypeName.IndexOf('`')]}<{genericArgs}>";
    }
}