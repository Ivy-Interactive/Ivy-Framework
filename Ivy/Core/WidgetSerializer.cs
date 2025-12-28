using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Ivy.Core.Helpers;

namespace Ivy.Core;

public static class WidgetSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers = { AddDefaultValueComparison }
        },
        Converters =
        {
            new JsonEnumConverter(),
            new ValueTupleConverterFactory()
        }
    };

    // Cache for default instances used by JSON serialization
    private static readonly ConcurrentDictionary<Type, object?> DefaultInstanceCache = new();

    private static bool ValuesAreEqual(object? a, object? b)
    {
        if (Equals(a, b)) return true;

        if (a is Array arrA && b is Array arrB)
        {
            if (arrA.Length != arrB.Length) return false;
            for (int i = 0; i < arrA.Length; i++)
            {
                if (!ValuesAreEqual(arrA.GetValue(i), arrB.GetValue(i)))
                    return false;
            }
            return true;
        }

        return false;
    }

    private static void AddDefaultValueComparison(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object)
            return;

        var defaultInstance = DefaultInstanceCache.GetOrAdd(typeInfo.Type, static t =>
        {
            var ctor = t.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                Type.EmptyTypes,
                null);

            if (ctor == null)
                return null;

            try
            {
                return ctor.Invoke(null);
            }
            catch
            {
                return null;
            }
        });

        if (defaultInstance == null)
            return;

        foreach (var property in typeInfo.Properties)
        {
            if (property.Get == null)
                continue;

            var defaultValue = property.Get(defaultInstance);
            property.ShouldSerialize = (_, currentValue) => !ValuesAreEqual(currentValue, defaultValue);
        }
    }

    private static readonly ConcurrentDictionary<Type, SerializationTypeMetadata> MetadataCache = new();

    private sealed record PropInfo(PropertyInfo Property, PropAttribute Attribute, string CamelCaseName);

    private sealed record EventInfo(PropertyInfo Property);

    private sealed record SerializationTypeMetadata(
        string TypeName,
        PropInfo[] PropProperties,
        EventInfo[] EventProperties,
        IWidget? DefaultInstance
    );

    private static SerializationTypeMetadata GetMetadata(Type type)
    {
        return MetadataCache.GetOrAdd(type, static t =>
        {
            var allProperties = t.GetProperties();

            var propProperties = allProperties
                .Select(p => (Property: p, Attribute: p.GetCustomAttribute<PropAttribute>()))
                .Where(x => x.Attribute != null)
                .Select(x => new PropInfo(x.Property, x.Attribute!, Utils.PascalCaseToCamelCase(x.Property.Name)))
                .ToArray();

            var eventProperties = allProperties
                .Where(p => p.GetCustomAttribute<EventAttribute>() != null)
                .Select(p => new EventInfo(p))
                .ToArray();

            IWidget? defaultInstance = null;
            var defaultCtor = t.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                Type.EmptyTypes,
                null);

            if (defaultCtor != null)
            {
                try
                {
                    defaultInstance = defaultCtor.Invoke(null) as IWidget;
                }
                catch
                {
                    // Ignore construction failures - we'll just not have default values
                }
            }

            var typeName = CleanTypeName(t);

            return new SerializationTypeMetadata(typeName, propProperties, eventProperties, defaultInstance);
        });
    }

    public static string CleanTypeName(Type t)
    {
        return t.Namespace + "." + Utils.CleanGenericNotation(t.Name);
    }

    public static JsonNode Serialize(IWidget widget)
    {
        var children = widget.Children;

        foreach (var child in children)
        {
            if (child is not IWidget)
                throw new InvalidOperationException("Only widgets can be serialized.");
        }

        var type = widget.GetType();
        var metadata = GetMetadata(type);

        // Serialize children
        var childrenArray = new JsonArray();
        foreach (var child in children)
        {
            childrenArray.Add(Serialize((IWidget)child));
        }

        var json = new JsonObject
        {
            ["id"] = widget.Id,
            ["type"] = metadata.TypeName,
            ["children"] = childrenArray
        };

        // Serialize props using cached metadata
        var props = new JsonObject();
        foreach (var propInfo in metadata.PropProperties)
        {
            var value = GetPropertyValue(widget, propInfo.Property, propInfo.Attribute);

            // Skip properties that match their default values
            if (metadata.DefaultInstance != null)
            {
                var defaultValue = GetPropertyValue(metadata.DefaultInstance, propInfo.Property, propInfo.Attribute);
                if (ValuesAreEqual(value, defaultValue))
                    continue;
            }
            else if (value == null)
            {
                continue;
            }

            props[propInfo.CamelCaseName] = JsonSerializer.SerializeToNode(value, SerializerOptions);
        }
        json["props"] = props;

        if (metadata.EventProperties.Length > 0)
        {
            var eventsArray = new JsonArray();
            foreach (var eventInfo in metadata.EventProperties)
            {
                if (eventInfo.Property.GetValue(widget) != null)
                    eventsArray.Add(JsonValue.Create(eventInfo.Property.Name));
            }
            json["events"] = eventsArray;
        }

        return json;
    }

    private static object? GetPropertyValue(IWidget widget, PropertyInfo property, PropAttribute attribute)
    {
        if (attribute.IsAttached)
        {
            if (!property.PropertyType.IsArray || !property.PropertyType.GetElementType()!.IsGenericType)
                throw new InvalidOperationException("Attached properties must be arrays of nullable types.");

            var children = widget.Children;
            var attachedValues = new object?[children.Length];
            var widgetType = widget.GetType();
            var attachedName = attribute.AttachedName!;

            for (int i = 0; i < children.Length; i++)
            {
                if (children[i] is IWidget childWidget)
                {
                    attachedValues[i] = childWidget.GetAttachedValue(widgetType, attachedName);
                }
            }
            return attachedValues;
        }

        return property.GetValue(widget);
    }
}
