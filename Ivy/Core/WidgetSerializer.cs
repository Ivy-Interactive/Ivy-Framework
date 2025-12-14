using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Ivy.Core.Helpers;
using Ivy.Widgets.Inputs;

namespace Ivy.Core;

public static class WidgetSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new JsonEnumConverter(),
            new ValueTupleConverterFactory(),
            new PrefixSuffixJsonConverterFactory() //todo: should be removed later
        }
    };

    // Cached metadata per widget type to avoid repeated reflection
    private static readonly ConcurrentDictionary<Type, SerializationTypeMetadata> MetadataCache = new();

    private sealed record PropInfo(PropertyInfo Property, PropAttribute Attribute, string CamelCaseName);

    private sealed record EventInfo(PropertyInfo Property);

    private sealed record SerializationTypeMetadata(
        string TypeName,
        PropInfo[] PropProperties,
        EventInfo[] EventProperties
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

            var typeName = t.Namespace + "." + Utils.CleanGenericNotation(t.Name);

            return new SerializationTypeMetadata(typeName, propProperties, eventProperties);
        });
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
            if (value == null)
                continue;
            props[propInfo.CamelCaseName] = JsonSerializer.SerializeToNode(value, SerializerOptions);
        }
        json["props"] = props;

        if (metadata.EventProperties.Length > 0)
        {
            var eventsArray = new JsonArray();
            foreach (var eventInfo in metadata.EventProperties)
            {
                if (eventInfo.Property.GetValue(widget) != null)
                    eventsArray.Add(eventInfo.Property.Name);
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
