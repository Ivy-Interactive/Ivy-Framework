using System.Net;

namespace Ivy.Core.Server;

/// <summary>
/// Decides which origins the default CORS policy reflects. Ivy serves its own frontend, so
/// production traffic is same-origin and never consults CORS at all; the cross-origin case that has
/// to keep working is the dev loop, where both sides are loopback.
/// </summary>
internal static class CorsOriginPolicy
{
    /// <summary>
    /// True when the server binds a loopback address, in which case loopback origins are allowed
    /// without configuration. Container and hosted servers bind "*" and land on false.
    /// </summary>
    internal static bool IsLoopbackBound(string? bindHost)
    {
        if (string.IsNullOrWhiteSpace(bindHost))
            return true;

        var host = bindHost.Trim();

        if (host is "*" or "+")
            return false;

        if (string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase))
            return true;

        return IPAddress.TryParse(host.Trim('[', ']'), out var address) && IPAddress.IsLoopback(address);
    }

    /// <summary>
    /// Normalizes configured origins to <c>scheme://host[:port]</c>, dropping anything that is not
    /// an absolute http/https URL. Wildcards are not supported.
    /// </summary>
    internal static string[] NormalizeOrigins(IEnumerable<string>? origins)
    {
        if (origins == null)
            return [];

        return origins
            .Select(Normalize)
            .Where(origin => origin != null)
            .Select(origin => origin!)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    /// <summary>
    /// True when <paramref name="origin"/> is explicitly listed in <paramref name="allowed"/>, or
    /// when it is a loopback origin and <paramref name="allowLoopback"/> is set. Anything that is
    /// not an absolute http/https URL is rejected outright, which covers <c>Origin: null</c>,
    /// <c>file://</c> and extension schemes.
    /// </summary>
    internal static bool IsOriginAllowed(string? origin, IReadOnlyList<string> allowed, bool allowLoopback)
    {
        var normalized = Normalize(origin);
        if (normalized == null)
            return false;

        for (var i = 0; i < allowed.Count; i++)
        {
            if (string.Equals(normalized, Normalize(allowed[i]), StringComparison.Ordinal))
                return true;
        }

        return allowLoopback && new Uri(normalized).IsLoopback;
    }

    private static string? Normalize(string? origin)
    {
        if (string.IsNullOrWhiteSpace(origin))
            return null;

        if (!Uri.TryCreate(origin.Trim(), UriKind.Absolute, out var uri))
            return null;

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            return null;

        // GetLeftPart lower-cases the scheme and host and elides a default port, so
        // "HTTP://LocalHost:80" and "http://localhost" compare equal.
        return uri.GetLeftPart(UriPartial.Authority);
    }
}
