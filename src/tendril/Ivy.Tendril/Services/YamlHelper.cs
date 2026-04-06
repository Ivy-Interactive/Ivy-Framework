using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Ivy.Tendril.Services;

/// <summary>
/// Shared YAML serialization helpers for consistent configuration handling.
/// All services use these static instances instead of creating their own builders.
/// </summary>
public static class YamlHelper
{
    /// <summary>
    /// Lenient deserializer with camelCase naming and unmatched property ignoring.
    /// Use this for loading config files where forward/backward compatibility is needed.
    /// </summary>
    public static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    /// <summary>
    /// Standard serializer with camelCase naming.
    /// </summary>
    public static readonly ISerializer Serializer = new SerializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    /// <summary>
    /// Compact serializer that omits default values for cleaner output.
    /// Use this for writing config files where brevity is desired.
    /// </summary>
    public static readonly ISerializer SerializerCompact = new SerializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults)
        .Build();
}
