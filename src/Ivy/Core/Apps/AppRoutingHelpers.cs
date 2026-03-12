using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ivy.Core.Apps;

public class RoutingConstantData
{
    [JsonPropertyName("excludedPaths")]
    public string[] ExcludedPaths { get; set; } = [];

    [JsonPropertyName("staticFileExtensions")]
    public string[] StaticFileExtensions { get; set; } = [];
}

[JsonSerializable(typeof(RoutingConstantData))]
internal partial class RoutingConstantDataContext : JsonSerializerContext;

public static class AppRoutingHelpers
{
    public static string[] ExcludedPaths => RoutingConstants.ExcludedPaths;

    private static readonly RoutingConstantData RoutingConstants;

    static AppRoutingHelpers()
    {
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("RoutingConstants")!;
        RoutingConstants = JsonSerializer.Deserialize(stream, RoutingConstantDataContext.Default.RoutingConstantData)!;
    }

    // PRECONDITION: RoutingConstants.ExcludedPaths have already been added to server.ReservedPaths.
    public static bool ValidateAppId(this global::Ivy.Server server, string appId)
    {
        var path = "/" + appId.Trim('/');

        // Invalid if path is empty
        if (string.IsNullOrEmpty(appId))
        {
            return false;
        }

        // Invalid if path starts with any excluded pattern (must be exact segment match)
        if (server.ReservedPaths.Contains(path) ||
            server.ReservedPaths.Any(reserved => path.StartsWith(reserved + "/", StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        // Invalid if path has a static file extension
        if (RoutingConstants.StaticFileExtensions.Any(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        return true;
    }
}
