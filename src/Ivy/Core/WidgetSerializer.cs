using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using Ivy.Core.Helpers;

namespace Ivy.Core;

public static class WidgetSerializer
{
    internal static readonly JsonSerializerOptions SerializerOptions = new()
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

            // Never omit a `required` property. Its "default" is whatever the
            // parameterless constructor happens to leave behind, which for a
            // value/enum type is the zero value — e.g. DataTableColumn.ColType
            // defaults to (ColType)0 == ColType.Number, so every numeric column
            // would silently drop its `type` and the client would treat it as
            // text. A required member is mandatory by declaration; always emit
            // it.
            if (property.IsRequired)
                continue;

            var defaultValue = property.Get(defaultInstance);
            property.ShouldSerialize = (_, currentValue) => !ValuesAreEqual(currentValue, defaultValue);
        }
    }

    private static readonly ConcurrentDictionary<Type, SerializationTypeMetadata> MetadataCache = new();

    private sealed record PropInfo(PropertyInfo Property, PropAttribute Attribute, string CamelCaseName, Func<IWidget, object?> Getter);

    private sealed record EventInfo(PropertyInfo Property, Func<IWidget, object?> Getter);

    private sealed record SerializationTypeMetadata(
        string TypeName,
        PropInfo[] PropProperties,
        EventInfo[] EventProperties,
        IWidget? DefaultInstance
    );

    private static Func<IWidget, object?> CreateGetter(Type type, PropertyInfo property)
    {
        var param = Expression.Parameter(typeof(IWidget), "w");
        var cast = Expression.Convert(param, type);
        var prop = Expression.Property(cast, property);
        var convertProp = Expression.Convert(prop, typeof(object));
        return Expression.Lambda<Func<IWidget, object?>>(convertProp, param).Compile();
    }

    private static SerializationTypeMetadata GetMetadata(Type type)
    {
        return MetadataCache.GetOrAdd(type, static t =>
        {
            var allProperties = t.GetProperties();

            var propProperties = allProperties
                .Select(p => (Property: p, Attribute: p.GetCustomAttribute<PropAttribute>()))
                .Where(x => x.Attribute != null)
                .Select(x => new PropInfo(x.Property, x.Attribute!, Utils.PascalCaseToCamelCase(x.Property.Name), CreateGetter(t, x.Property)))
                .ToArray();

            var eventProperties = allProperties
                .Where(p => p.GetCustomAttribute<EventAttribute>() != null)
                .Select(p => new EventInfo(p, CreateGetter(t, p)))
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
            var value = GetPropertyValue(widget, propInfo);

            // Skip properties that match their default values (unless AlwaysSerialize is set)
            if (!propInfo.Attribute.AlwaysSerialize && metadata.DefaultInstance != null)
            {
                var defaultValue = GetPropertyValue(metadata.DefaultInstance, propInfo);
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
                if (eventInfo.Getter(widget) != null)
                    eventsArray.Add(JsonValue.Create(eventInfo.Property.Name));
            }
            json["events"] = eventsArray;
        }

#if DEBUG
        if (widget is AbstractWidget abstractWidget)
        {
            var callSiteObj = new JsonObject();

            if (widget.Path != null)
            {
                callSiteObj["path"] = widget.Path;
            }

            if (abstractWidget.CallSite is { } callSite)
            {
                callSiteObj["filePath"] = callSite.FilePath;
                callSiteObj["lineNumber"] = callSite.LineNumber;
                callSiteObj["memberName"] = callSite.MemberName;
                callSiteObj["declaringType"] = callSite.DeclaringType;
            }

            if (callSiteObj.Count > 0)
            {
                json["callSite"] = callSiteObj;
            }
        }
#endif

        return json;
    }

    private static object? GetPropertyValue(IWidget widget, PropInfo propInfo)
    {
        var attribute = propInfo.Attribute;
        if (attribute.IsAttached)
        {
            var property = propInfo.Property;
            if (!property.PropertyType.IsArray)
                throw new InvalidOperationException("Attached properties must be arrays.");

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

        return propInfo.Getter(widget);
    }
}
