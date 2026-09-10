using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using Ivy.Core.Helpers;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Derives the JSON Schema advertised to browser agents from a tool's argument type.
/// </summary>
internal static class WebMcpSchemaGenerator
{
    private static readonly ConcurrentDictionary<Type, string> Cache = new();

    /// <summary>
    /// Returns the JSON Schema for <paramref name="argumentsType"/> as a JSON string. Schemas are
    /// generated with camelCase property names so they match both what the agent sends and how the
    /// arguments are deserialized back into the tool's argument type.
    /// </summary>
    public static string GetSchemaJson(Type argumentsType) =>
        Cache.GetOrAdd(argumentsType, static type =>
        {
            var exporterOptions = new JsonSchemaExporterOptions
            {
                TransformSchemaNode = static (context, schema) =>
                {
                    if (schema is not JsonObject node) return schema;

                    var isRoot = context.PropertyInfo == null;

                    // JsonSchemaExporter does not read [Description] on its own, but those
                    // descriptions are what tell the agent what each argument means.
                    var description = isRoot
                        ? GetDescription(context.TypeInfo.Type)
                        : GetDescription(context.PropertyInfo!.AttributeProvider);

                    if (description != null && !node.ContainsKey("description"))
                    {
                        node["description"] = description;
                    }

                    // The exporter marks a reference type as nullable, which at the root would
                    // advertise `"type": ["object", "null"]`. Agents expect a plain object there.
                    if (isRoot) StripNullFromType(node);

                    return node;
                }
            };

            return JsonSchemaExporter
                .GetJsonSchemaAsNode(JsonHelper.CamelCaseOptions, type, exporterOptions)
                .ToJsonString();
        });

    private static void StripNullFromType(JsonObject node)
    {
        if (node["type"] is not JsonArray types) return;

        var named = types
            .OfType<JsonValue>()
            .Select(v => v.GetValue<string>())
            .Where(t => t != "null")
            .ToArray();

        if (named.Length == 1)
        {
            node["type"] = named[0];
        }
    }

    private static string? GetDescription(ICustomAttributeProvider? provider) =>
        provider?.GetCustomAttributes(typeof(DescriptionAttribute), inherit: true)
            .OfType<DescriptionAttribute>()
            .FirstOrDefault()?.Description;

    private static string? GetDescription(Type type) =>
        type.GetCustomAttribute<DescriptionAttribute>()?.Description;
}
