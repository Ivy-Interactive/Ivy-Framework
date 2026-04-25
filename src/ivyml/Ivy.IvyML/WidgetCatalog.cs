using System.Reflection;
using System.Text;
using Ivy.Core;

namespace Ivy.IvyML;

public record WidgetPropInfo(string Name, string Type, bool IsNullable, string? DefaultValue, string[]? EnumValues);

public record WidgetEventInfo(string Name);

public record WidgetSlotInfo(string Name);

public record WidgetInfo(string Name, WidgetPropInfo[] Props, WidgetEventInfo[] Events, WidgetSlotInfo[] Slots);

public static class WidgetCatalog
{
    private static readonly Lazy<Dictionary<string, WidgetInfo>> Widgets = new(BuildCatalog);

    public static IReadOnlyDictionary<string, WidgetInfo> All => Widgets.Value;

    public static WidgetInfo? Get(string name) =>
        Widgets.Value.GetValueOrDefault(name) ??
        Widgets.Value.FirstOrDefault(kv => kv.Key.Equals(name, StringComparison.OrdinalIgnoreCase)).Value;

    public static string[] ListNames() =>
        Widgets.Value.Keys.Order().ToArray();

    private static Dictionary<string, WidgetInfo> BuildCatalog()
    {
        var catalog = new Dictionary<string, WidgetInfo>(StringComparer.OrdinalIgnoreCase);
        var assembly = typeof(AbstractWidget).Assembly;

        foreach (var type in assembly.GetTypes())
        {
            if (type.IsGenericTypeDefinition || type.IsEnum ||
                type.IsInterface || type.IsNested)
                continue;

            if (!typeof(AbstractWidget).IsAssignableFrom(type))
                continue;

            var props = GetProps(type);
            var events = GetEvents(type);
            var slots = GetSlots(type);

            if (props.Length == 0 && events.Length == 0 && slots.Length == 0)
                continue;

            var name = type.Name;
            if (type.IsAbstract && name.EndsWith("Base"))
                name = name[..^4];

            if (catalog.ContainsKey(name))
                continue;

            catalog[name] = new WidgetInfo(name, props, events, slots);
        }

        return catalog;
    }

    private static WidgetPropInfo[] GetProps(Type type)
    {
        object? defaultInstance = null;
        if (!type.IsAbstract)
        {
            var ctor = type.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null, Type.EmptyTypes, null);
            if (ctor != null)
            {
                try { defaultInstance = ctor.Invoke(null); }
                catch { /* ignore */ }
            }
        }

        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute<PropAttribute>() != null)
            .Where(p => p.GetCustomAttribute<PropAttribute>()!.AttachedName == null)
            .Select(p => BuildPropInfo(p, defaultInstance))
            .ToArray();
    }

    private static WidgetPropInfo BuildPropInfo(PropertyInfo prop, object? defaultInstance)
    {
        var propType = prop.PropertyType;
        var underlying = Nullable.GetUnderlyingType(propType);
        var isNullable = underlying != null;
        var effectiveType = underlying ?? propType;

        if (effectiveType.IsGenericType && effectiveType.GetGenericTypeDefinition() == typeof(Responsive<>))
            effectiveType = effectiveType.GetGenericArguments()[0];

        string? defaultValue = null;
        if (defaultInstance != null)
        {
            try
            {
                var val = prop.GetValue(defaultInstance);
                if (val != null)
                    defaultValue = FormatDefault(val, effectiveType);
            }
            catch { /* ignore */ }
        }

        string[]? enumValues = null;
        var enumType = effectiveType;
        if (Nullable.GetUnderlyingType(enumType) is { } innerEnum)
            enumType = innerEnum;
        if (enumType.IsEnum)
            enumValues = Enum.GetNames(enumType);

        return new WidgetPropInfo(prop.Name, FormatTypeName(effectiveType), isNullable, defaultValue, enumValues);
    }

    private static WidgetEventInfo[] GetEvents(Type type) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute<EventAttribute>() != null)
            .Select(p => new WidgetEventInfo(p.Name))
            .ToArray();

    private static WidgetSlotInfo[] GetSlots(Type type) =>
        type.GetCustomAttributes<SlotAttribute>()
            .Select(a => new WidgetSlotInfo(a.Name))
            .ToArray();

    private static string FormatTypeName(Type type)
    {
        if (type == typeof(string)) return "string";
        if (type == typeof(bool)) return "bool";
        if (type == typeof(int)) return "int";
        if (type == typeof(float)) return "float";
        if (type == typeof(double)) return "double";
        if (type == typeof(char)) return "char";
        if (type == typeof(Size)) return "Size";
        if (type == typeof(Thickness)) return "Thickness";
        if (type.IsEnum) return type.Name;
        if (type.IsArray) return FormatTypeName(type.GetElementType()!) + "[]";
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Responsive<>))
            return FormatTypeName(type.GetGenericArguments()[0]);
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            return FormatTypeName(Nullable.GetUnderlyingType(type)!);
        if (type.IsGenericType)
            return type.Name.Split('`')[0];
        return type.Name;
    }

    private static string? FormatDefault(object value, Type type)
    {
        if (type.IsEnum) return value.ToString();
        if (value is bool b) return b ? "true" : "false";
        if (value is string s) return s.Length > 0 ? $"\"{s}\"" : null;
        if (value is char c) return $"'{c}'";
        if (value is int i) return i != 0 ? i.ToString() : null;
        if (value is float f) return f != 0 ? f.ToString() : null;
        if (value is double d) return d != 0 ? d.ToString() : null;
        return null;
    }

    public static string FormatWidgetList()
    {
        var sb = new StringBuilder();
        foreach (var name in ListNames())
            sb.AppendLine(name);
        return sb.ToString().TrimEnd();
    }

    public static string FormatWidgetDetail(WidgetInfo widget)
    {
        var sb = new StringBuilder();
        sb.AppendLine(widget.Name);
        sb.AppendLine();

        if (widget.Props.Length > 0)
        {
            sb.AppendLine("Props:");
            foreach (var prop in widget.Props)
            {
                var def = prop.DefaultValue != null ? $" = {prop.DefaultValue}" : "";
                var nullable = prop.IsNullable ? "?" : "";
                sb.Append($"  {prop.Name} : {prop.Type}{nullable}{def}");
                if (prop.EnumValues is { Length: > 0 })
                    sb.Append($" [{string.Join(", ", prop.EnumValues)}]");
                sb.AppendLine();
            }
        }

        if (widget.Slots.Length > 0)
        {
            if (widget.Props.Length > 0)
                sb.AppendLine();
            sb.AppendLine("Slots:");
            foreach (var slot in widget.Slots)
                sb.AppendLine($"  {slot.Name}");
        }

        if (widget.Events.Length > 0)
        {
            if (widget.Props.Length > 0 || widget.Slots.Length > 0)
                sb.AppendLine();
            sb.AppendLine("Events:");
            foreach (var evt in widget.Events)
                sb.AppendLine($"  {evt.Name}");
        }

        return sb.ToString().TrimEnd();
    }
}
