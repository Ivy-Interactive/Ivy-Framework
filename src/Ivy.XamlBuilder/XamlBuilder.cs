using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Xml.Linq;
using Ivy.Core;

namespace Ivy;

public class XamlBuilder
{
    private readonly Dictionary<string, Type> _typeMap;

    public XamlBuilder(params Assembly[] assemblies)
    {
        var assembliesToScan = assemblies.Length > 0
            ? assemblies
            : [typeof(AbstractWidget).Assembly];

        _typeMap = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        foreach (var assembly in assembliesToScan)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract || type.IsGenericTypeDefinition || type.IsEnum ||
                    type.IsInterface || type.IsNested)
                    continue;

                if (!HasUsableConstructor(type))
                    continue;

                _typeMap[type.Name] = type;
            }
        }
    }

    public AbstractWidget Build(string xaml)
    {
        var doc = XDocument.Parse(xaml);
        return BuildWidget(doc.Root!);
    }

    private AbstractWidget BuildWidget(XElement element)
    {
        var name = element.Name.LocalName;

        if (!_typeMap.TryGetValue(name, out var type))
            throw new InvalidOperationException($"Unknown type: {name}");

        if (!typeof(AbstractWidget).IsAssignableFrom(type))
            throw new InvalidOperationException($"Type '{name}' is not a widget.");

        var widget = (AbstractWidget)CreateInstance(type);
        SetAttributes(widget, element);

        var children = new List<object>();
        foreach (var child in element.Elements())
        {
            if (child.Name.LocalName.Contains('.'))
            {
                ProcessPropertyElement(child, widget);
            }
            else if (TrySetJsonProperty(widget, child))
            {
                // Child was a JSON data element (e.g. <Data><![CDATA[...]]></Data>)
            }
            else
            {
                children.Add(BuildWidget(child));
            }
        }

        widget.Children = children.ToArray();
        return widget;
    }

    private void ProcessPropertyElement(XElement propElement, object owner)
    {
        var name = propElement.Name.LocalName;
        var dotIndex = name.IndexOf('.');
        var propertyName = name[(dotIndex + 1)..];

        var prop = FindProperty(owner, propertyName)
            ?? throw new InvalidOperationException(
                $"Unknown property '{propertyName}' on {owner.GetType().Name}.");

        var propType = prop.PropertyType;

        if (propType == typeof(object))
        {
            var children = propElement.Elements().ToList();
            if (children.Count == 1)
            {
                SetProperty(owner, prop, BuildObject(children[0], typeof(object)));
            }
            return;
        }

        if (propType.IsArray)
        {
            var elementType = propType.GetElementType()!;
            var items = new List<object>();
            foreach (var child in propElement.Elements())
                items.Add(BuildObject(child, elementType));

            var array = Array.CreateInstance(elementType, items.Count);
            for (var i = 0; i < items.Count; i++)
                array.SetValue(items[i], i);

            SetProperty(owner, prop, array);
        }
        else
        {
            object? value;
            if (propElement.HasAttributes || !propElement.HasElements)
            {
                value = CreateInstance(propType);
                SetAttributes(value, propElement);
                foreach (var child in propElement.Elements())
                {
                    if (child.Name.LocalName.Contains('.'))
                        ProcessPropertyElement(child, value);
                }
            }
            else if (propElement.Elements().Count() == 1)
            {
                value = BuildObject(propElement.Elements().First(), propType);
            }
            else
            {
                value = CreateInstance(propType);
            }

            SetProperty(owner, prop, value);
        }
    }

    private object BuildObject(XElement element, Type expectedType)
    {
        var resolvedType = _typeMap.TryGetValue(element.Name.LocalName, out var mappedType)
            ? mappedType
            : expectedType;

        if (!expectedType.IsAssignableFrom(resolvedType))
            throw new InvalidOperationException(
                $"Type '{resolvedType.Name}' is not assignable to '{expectedType.Name}'.");

        var obj = CreateInstance(resolvedType);
        SetAttributes(obj, element);

        foreach (var child in element.Elements())
        {
            if (child.Name.LocalName.Contains('.'))
                ProcessPropertyElement(child, obj);
        }

        return obj;
    }

    private void SetAttributes(object target, XElement element)
    {
        foreach (var attr in element.Attributes())
        {
            var prop = FindProperty(target, attr.Name.LocalName)
                ?? throw new InvalidOperationException(
                    $"Unknown property '{attr.Name.LocalName}' on {target.GetType().Name}.");

            var value = ConvertValue(attr.Value, prop.PropertyType);
            SetProperty(target, prop, value);
        }
    }

    private static PropertyInfo? FindProperty(object target, string name)
    {
        var type = target.GetType();
        var isWidget = target is AbstractWidget;
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in props)
        {
            if (!string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))
                continue;

            if (isWidget)
            {
                if (prop.GetCustomAttribute<PropAttribute>() != null)
                    return prop;
            }
            else
            {
                return prop;
            }
        }

        return null;
    }

    private static void SetProperty(object target, PropertyInfo prop, object? value)
    {
        if (prop.CanWrite)
        {
            prop.SetValue(target, value);
        }
        else
        {
            var field = target.GetType().GetField(
                $"<{prop.Name}>k__BackingField",
                BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(target, value);
        }
    }

    private static object CreateInstance(Type type)
    {
        var parameterlessCtor = type.GetConstructor(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            null, Type.EmptyTypes, null);

        if (parameterlessCtor != null)
            return parameterlessCtor.Invoke(null);

        var ctor = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .FirstOrDefault(c => c.GetParameters().Length > 0 &&
                                 c.GetParameters().All(p => p.HasDefaultValue));

        if (ctor != null)
        {
            var defaults = ctor.GetParameters().Select(p => p.DefaultValue).ToArray();
            return ctor.Invoke(defaults);
        }

        throw new InvalidOperationException($"No suitable constructor found for type '{type.Name}'.");
    }

    private static bool HasUsableConstructor(Type type)
    {
        foreach (var ctor in type.GetConstructors(
                     BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            var parameters = ctor.GetParameters();
            if (parameters.Length == 0)
                return true;
            if (parameters.All(p => p.HasDefaultValue))
                return true;
        }
        return false;
    }

    private static object? ConvertValue(string value, Type targetType)
    {
        var underlyingType = Nullable.GetUnderlyingType(targetType);
        if (underlyingType != null)
            return ConvertValue(value, underlyingType);

        if (targetType == typeof(string))
            return value;

        if (targetType == typeof(bool))
            return bool.Parse(value);

        if (targetType == typeof(int))
            return int.Parse(value, CultureInfo.InvariantCulture);

        if (targetType == typeof(float))
            return float.Parse(value, CultureInfo.InvariantCulture);

        if (targetType == typeof(double))
            return double.Parse(value, CultureInfo.InvariantCulture);

        if (targetType.IsEnum)
            return Enum.Parse(targetType, value, ignoreCase: true);

        if (targetType == typeof(Size))
            return ParseSize(value);

        if (targetType == typeof(Thickness))
            return ParseThickness(value);

        if (targetType == typeof(int[]))
            return value.Split(',')
                .Select(s => int.Parse(s.Trim(), CultureInfo.InvariantCulture))
                .ToArray();

        throw new InvalidOperationException($"Cannot convert '{value}' to {targetType.Name}.");
    }

    private static bool TrySetJsonProperty(AbstractWidget widget, XElement child)
    {
        var prop = FindProperty(widget, child.Name.LocalName);
        if (prop == null || child.HasElements || string.IsNullOrWhiteSpace(child.Value))
            return false;

        var jsonText = child.Value.Trim();
        if (!jsonText.StartsWith('['))
            return false;

        var data = JsonSerializer.Deserialize<Dictionary<string, object>[]>(jsonText, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = false
        });

        if (data != null)
        {
            for (var i = 0; i < data.Length; i++)
            {
                var converted = new Dictionary<string, object>();
                foreach (var kvp in data[i])
                    converted[kvp.Key] = kvp.Value is JsonElement je ? ConvertJsonElement(je) : kvp.Value;
                data[i] = converted;
            }

            SetProperty(widget, prop, data);
        }

        return true;
    }

    private static object ConvertJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString()!,
            JsonValueKind.Number when element.TryGetInt32(out var i) => (double)i,
            JsonValueKind.Number => element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null!,
            _ => element.GetRawText()
        };
    }

    private static Size ParseSize(string value)
    {
        if (string.Equals(value, "Full", StringComparison.OrdinalIgnoreCase))
            return Size.Full();
        if (string.Equals(value, "Fit", StringComparison.OrdinalIgnoreCase))
            return Size.Fit();
        if (string.Equals(value, "Auto", StringComparison.OrdinalIgnoreCase))
            return Size.Auto();
        if (string.Equals(value, "Screen", StringComparison.OrdinalIgnoreCase))
            return Size.Screen();
        if (string.Equals(value, "Half", StringComparison.OrdinalIgnoreCase))
            return Size.Half();

        if (value.EndsWith('%'))
        {
            var num = float.Parse(value[..^1], CultureInfo.InvariantCulture);
            return Size.Fraction(num / 100f);
        }

        if (value.EndsWith("px", StringComparison.OrdinalIgnoreCase))
        {
            var num = int.Parse(value[..^2], CultureInfo.InvariantCulture);
            return Size.Px(num);
        }

        if (value.EndsWith("rem", StringComparison.OrdinalIgnoreCase))
        {
            var num = int.Parse(value[..^3], CultureInfo.InvariantCulture);
            return Size.Rem(num);
        }

        if (int.TryParse(value, CultureInfo.InvariantCulture, out var units))
            return Size.Units(units);

        throw new InvalidOperationException($"Cannot parse Size from '{value}'.");
    }

    private static Thickness ParseThickness(string value)
    {
        var parts = value.Split(',')
            .Select(s => int.Parse(s.Trim(), CultureInfo.InvariantCulture))
            .ToArray();

        return parts.Length switch
        {
            1 => new Thickness(parts[0]),
            2 => new Thickness(parts[0], parts[1]),
            4 => new Thickness(parts[0], parts[1], parts[2], parts[3]),
            _ => throw new InvalidOperationException($"Cannot parse Thickness from '{value}'.")
        };
    }
}
