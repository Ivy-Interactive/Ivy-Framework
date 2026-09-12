using System.Net;
using System.Net.Sockets;

namespace Ivy.Core.Server;

/// <summary>
/// Decides whether Ivy supplies a default <c>AllowedHosts</c> list. Host header validation is the DNS
/// rebinding mitigation: a page on a hostname whose DNS answer flips to 127.0.0.1 is same-origin with
/// the server, so no CORS check runs and only the Host header separates it from a real local request.
/// </summary>
internal static class HostFilterPolicy
{
    private static readonly string[] LoopbackHosts = ["localhost", "127.0.0.1", "[::1]"];

    /// <summary>
    /// Returns the list to install, or null when Ivy must not touch the option: either the app
    /// configured <c>AllowedHosts</c> itself (ASP.NET Core reads that key only while the option list is
    /// empty, so filling it here would clobber the key), or the server does not bind loopback, where a
    /// proxy or tunnel may legitimately forward its own hostname.
    /// </summary>
    internal static string[]? ResolveDefaultAllowedHosts(string? bindHost, string? configuredAllowedHosts)
    {
        if (configuredAllowedHosts != null)
            return null;

        if (!CorsOriginPolicy.IsLoopbackBound(bindHost))
            return null;

        var trimmed = bindHost?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return LoopbackHosts;

        // A bind on another loopback literal (127.0.0.2, ::1 without brackets) has to be reachable
        // under its own name too. HostString compares IPv6 in bracketed form.
        var pattern = IPAddress.TryParse(trimmed.Trim('[', ']'), out var address)
                      && address.AddressFamily == AddressFamily.InterNetworkV6
            ? $"[{address}]"
            : trimmed;

        return LoopbackHosts.Contains(pattern, StringComparer.OrdinalIgnoreCase)
            ? LoopbackHosts
            : [.. LoopbackHosts, pattern];
    }
}
