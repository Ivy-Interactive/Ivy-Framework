using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Ivy.Core.Helpers;

/// <summary>
/// Provides shared JSON serialization options for AOT/single-file compatibility.
/// </summary>
public static class JsonHelper
{
    /// <summary>
    /// Default JsonSerializerOptions with reflection-based serialization enabled.
    /// Use this for dynamic/generic serialization scenarios.
    /// </summary>
    public static JsonSerializerOptions DefaultOptions { get; } = new()
    {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };

    /// <summary>
    /// JsonSerializerOptions with camelCase naming and reflection-based serialization.
    /// </summary>
    public static JsonSerializerOptions CamelCaseOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };
}
