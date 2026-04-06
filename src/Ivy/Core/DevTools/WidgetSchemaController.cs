using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Ivy.Core.ExternalWidgets;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Core.DevTools;

/// <summary>
/// Serves a JSON Schema describing every registered widget type and its props.
/// Only available when the server is started with --enable-dev-tools.
/// </summary>
[ApiController]
[Route("ivy/dev-tools")]
public class WidgetSchemaController(ServerArgs args) : ControllerBase
{
    [HttpGet("widget-schema")]
    public IActionResult GetWidgetSchema()
    {
        if (!args.EnableDevTools)
            return NotFound("Widget schema is only available with --enable-dev-tools");

        var schema = WidgetSchemaGenerator.Generate();
        return Content(schema.ToJsonString(new JsonSerializerOptions { WriteIndented = true }), "application/json");
    }

    [HttpGet("env-info")]
    public IActionResult GetEnvInfo()
    {
        if (!args.EnableDevTools)
            return NotFound("Env info is only available with --enable-dev-tools");

        var tendrilHome = Environment.GetEnvironmentVariable("TENDRIL_HOME");
        var hasTendril = false;

        if (!string.IsNullOrEmpty(tendrilHome) && Directory.Exists(tendrilHome))
        {
            var inboxPath = Path.Combine(tendrilHome, "Inbox");
            hasTendril = Directory.Exists(inboxPath);
        }

        return Ok(new { tendrilDetected = hasTendril, tendrilHome });
    }
}

internal static class WidgetSchemaGenerator
{
    private static readonly ConcurrentDictionary<Type, object?> DefaultInstanceCache = new();

    public static JsonObject Generate()
    {
        var root = new JsonObject
        {
            ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
            ["description"] = "Ivy widget type definitions with all available props, types, and defaults",
            ["generatedAt"] = DateTime.UtcNow.ToString("O"),
        };

        var widgets = new JsonObject();
        var definitions = new JsonObject();

        // Discover all widget types from loaded assemblies
        var widgetTypes = DiscoverWidgetTypes();

        foreach (var type in widgetTypes.OrderBy(t => CleanTypeName(t)))
        {
            var typeName = CleanTypeName(type);
            var widgetSchema = BuildWidgetSchema(type, definitions);
            if (widgetSchema != null)
                widgets[typeName] = widgetSchema;
        }

        root["widgets"] = widgets;
        if (definitions.Count > 0)
            root["$defs"] = definitions;

        return root;
    }

    private static List<Type> DiscoverWidgetTypes()
    {
        var widgetTypes = new List<Type>();
        var seen = new HashSet<string>();

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (IsSystemAssembly(assembly)) continue;

            Type[] types;
            try { types = assembly.GetTypes(); }
            catch { continue; }

            foreach (var type in types)
            {
                if (type.IsAbstract || type.IsInterface) continue;
                if (!typeof(IWidget).IsAssignableFrom(type)) continue;

                var name = CleanTypeName(type);
                if (seen.Add(name))
                    widgetTypes.Add(type);
            }
        }

        // Also include external widgets
        foreach (var (typeName, info) in ExternalWidgetRegistry.Instance.GetAll())
        {
            // External widgets are already in the assembly scan, but ensure they're included
            if (seen.Contains(typeName)) continue;

            var type = info.Assembly.GetTypes().FirstOrDefault(t => CleanTypeName(t) == typeName);
            if (type != null && seen.Add(typeName))
                widgetTypes.Add(type);
        }

        return widgetTypes;
    }

    private static JsonObject? BuildWidgetSchema(Type type, JsonObject definitions)
    {
        var allProperties = type.GetProperties();

        var propProperties = allProperties
            .Select(p => (Property: p, Attribute: p.GetCustomAttribute<PropAttribute>()))
            .Where(x => x.Attribute != null)
            .ToArray();

        var eventProperties = allProperties
            .Where(p => p.GetCustomAttribute<EventAttribute>() != null)
            .ToArray();

        if (propProperties.Length == 0 && eventProperties.Length == 0)
            return null;

        // Create default instance for default values
        object? defaultInstance = null;
        var defaultCtor = type.GetConstructor(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null, Type.EmptyTypes, null);
        if (defaultCtor != null)
        {
            try { defaultInstance = defaultCtor.Invoke(null); } catch { /* ignore */ }
        }

        var schema = new JsonObject
        {
            ["type"] = "object",
        };

        // Props
        var props = new JsonObject();
        foreach (var (prop, attr) in propProperties)
        {
            if (attr!.IsAttached) continue; // skip attached props for now

            var camelName = Utils.PascalCaseToCamelCase(prop.Name);
            var propSchema = BuildPropertySchema(prop.PropertyType, definitions);

            // Add default value
            if (defaultInstance != null)
            {
                try
                {
                    var defaultValue = prop.GetValue(defaultInstance);
                    if (defaultValue != null)
                    {
                        var defaultNode = SerializeDefault(defaultValue, prop.PropertyType);
                        if (defaultNode != null)
                            propSchema["default"] = defaultNode;
                    }
                }
                catch { /* ignore */ }
            }

            props[camelName] = propSchema;
        }
        schema["properties"] = props;

        // Events
        if (eventProperties.Length > 0)
        {
            var events = new JsonArray();
            foreach (var evtProp in eventProperties)
            {
                events.Add(evtProp.Name);
            }
            schema["events"] = events;
        }

        return schema;
    }

    private static JsonObject BuildPropertySchema(Type propertyType, JsonObject definitions)
    {
        // Handle nullable types
        var (underlyingType, isNullable) = UnwrapNullable(propertyType);

        var schema = BuildTypeSchema(underlyingType, definitions);

        if (isNullable)
        {
            schema["nullable"] = true;
        }

        return schema;
    }

    private static JsonObject BuildTypeSchema(Type type, JsonObject definitions)
    {
        // Enum
        if (type.IsEnum)
        {
            var defName = type.Name;
            if (!definitions.ContainsKey(defName))
            {
                var enumValues = new JsonArray();
                foreach (var val in Enum.GetValues(type))
                    enumValues.Add(val.ToString());

                definitions[defName] = new JsonObject
                {
                    ["type"] = "string",
                    ["enum"] = enumValues
                };
            }
            return new JsonObject { ["$ref"] = $"#/$defs/{defName}" };
        }

        // Primitives
        if (type == typeof(string))
            return new JsonObject { ["type"] = "string" };
        if (type == typeof(bool))
            return new JsonObject { ["type"] = "boolean" };
        if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte))
            return new JsonObject { ["type"] = "integer" };
        if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
            return new JsonObject { ["type"] = "number" };

        // Arrays / Lists
        if (type.IsArray)
        {
            var elementType = type.GetElementType()!;
            var (unwrapped, _) = UnwrapNullable(elementType);
            return new JsonObject
            {
                ["type"] = "array",
                ["items"] = BuildTypeSchema(unwrapped, definitions)
            };
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            var elementType = type.GetGenericArguments()[0];
            return new JsonObject
            {
                ["type"] = "array",
                ["items"] = BuildTypeSchema(elementType, definitions)
            };
        }

        // Well-known Ivy value types
        if (type == typeof(Size))
            return BuildSizeDefinition(definitions);
        if (type == typeof(Thickness))
            return BuildThicknessDefinition(definitions);

        // Complex object — try to describe its public properties
        if (type.IsClass || type.IsValueType)
        {
            var defName = type.Name;
            if (!definitions.ContainsKey(defName))
            {
                var objSchema = new JsonObject { ["type"] = "object" };
                var objProps = new JsonObject();

                // Placeholder to prevent infinite recursion
                definitions[defName] = objSchema;

                foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (!prop.CanRead) continue;
                    // Skip indexers
                    if (prop.GetIndexParameters().Length > 0) continue;
                    // Skip properties that would cause cycles (IWidget, object, etc.)
                    if (typeof(IWidget).IsAssignableFrom(prop.PropertyType)) continue;
                    if (prop.PropertyType == typeof(object)) continue;
                    if (prop.PropertyType == type) continue;

                    var camelName = Utils.PascalCaseToCamelCase(prop.Name);
                    objProps[camelName] = BuildPropertySchema(prop.PropertyType, definitions);
                }

                if (objProps.Count > 0)
                    objSchema["properties"] = objProps;
            }
            return new JsonObject { ["$ref"] = $"#/$defs/{defName}" };
        }

        return new JsonObject { ["type"] = "string", ["description"] = type.Name };
    }

    private static JsonObject BuildSizeDefinition(JsonObject definitions)
    {
        if (!definitions.ContainsKey("Size"))
        {
            definitions["Size"] = new JsonObject
            {
                ["description"] = "Flexible sizing value. Serialized as string: 'Type:Value,Min,Max'. Examples: 'Px:100', 'Units:2', 'Grow', 'Full', 'Fit'",
                ["type"] = "string",
            };
        }
        return new JsonObject { ["$ref"] = "#/$defs/Size" };
    }

    private static JsonObject BuildThicknessDefinition(JsonObject definitions)
    {
        if (!definitions.ContainsKey("Thickness"))
        {
            definitions["Thickness"] = new JsonObject
            {
                ["description"] = "Thickness/spacing value. Serialized as 'left,top,right,bottom'. Examples: '10,10,10,10', '5,0,5,0'",
                ["type"] = "string",
            };
        }
        return new JsonObject { ["$ref"] = "#/$defs/Thickness" };
    }

    private static (Type underlyingType, bool isNullable) UnwrapNullable(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type);
        if (underlying != null)
            return (underlying, true);

        // Reference types are inherently nullable, but we only flag nullable value types
        return (type, false);
    }

    private static JsonNode? SerializeDefault(object value, Type propertyType)
    {
        try
        {
            var (underlyingType, _) = UnwrapNullable(propertyType);

            if (underlyingType.IsEnum)
                return JsonValue.Create(value.ToString());
            if (value is bool b)
                return JsonValue.Create(b);
            if (value is int i)
                return JsonValue.Create(i);
            if (value is float f)
                return JsonValue.Create(f);
            if (value is double d)
                return JsonValue.Create(d);
            if (value is string s)
                return JsonValue.Create(s);

            // For complex types, use the Ivy serializer options
            return JsonSerializer.SerializeToNode(value, WidgetSerializer.SerializerOptions);
        }
        catch
        {
            return null;
        }
    }

    private static string CleanTypeName(Type t) =>
        t.Namespace + "." + Utils.CleanGenericNotation(t.Name);

    private static bool IsSystemAssembly(Assembly assembly)
    {
        var name = assembly.GetName().Name ?? "";
        return name.StartsWith("System") ||
               name.StartsWith("Microsoft") ||
               name.StartsWith("netstandard") ||
               name.StartsWith("mscorlib") ||
               name == "Anonymously Hosted DynamicMethods Assembly";
    }
}
