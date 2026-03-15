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

public enum AppIdValidationResult
{
    Valid,
    Empty,
    StartsWithSlash,
    UnsafeCharacters,
    ReservedPathConflict,
    StaticFileExtensionConflict
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

    public static AppIdValidationResult ValidateAppId(string appId, IReadOnlySet<string> reservedPaths)
    {
        if (string.IsNullOrEmpty(appId))
        {
            return AppIdValidationResult.Empty;
        }

        if (appId.StartsWith('/'))
        {
            return AppIdValidationResult.StartsWithSlash;
        }

        // Invalid if app ID contains unsafe characters
        if (!ValidationHelper.IsSafeAppId(appId))
        {
            return AppIdValidationResult.UnsafeCharacters;
        }

        var path = "/" + appId;

        // Invalid if path starts with any excluded pattern (must be exact segment match)
        if (reservedPaths.Contains(path) ||
            reservedPaths.Any(reserved => path.StartsWith(reserved + "/", StringComparison.OrdinalIgnoreCase)))
        {
            return AppIdValidationResult.ReservedPathConflict;
        }

        // Invalid if path has a static file extension
        if (RoutingConstants.StaticFileExtensions.Any(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
        {
            return AppIdValidationResult.StaticFileExtensionConflict;
        }

        return AppIdValidationResult.Valid;
    }
}
