namespace Ivy.Core.Server;

/// <summary>
/// Reads the IVY_TLS environment variable. The accepted token set is shared by the server and by
/// the desktop host, which each supply their own default for an unset value.
/// </summary>
internal static class TlsPolicy
{
    /// <summary>
    /// True when the value asks for TLS. An unset or empty value yields
    /// <paramref name="fallback"/>; any other unrecognized value is off, not a fallback.
    /// </summary>
    internal static bool IsEnabled(string? value, bool fallback) =>
        string.IsNullOrEmpty(value)
            ? fallback
            : value.ToLowerInvariant() is "1" or "true" or "yes" or "on";
}
