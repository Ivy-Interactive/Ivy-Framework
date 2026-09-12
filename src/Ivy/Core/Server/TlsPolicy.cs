namespace Ivy.Core.Server;

/// <summary>
/// Decides whether the server binds https or http. Explicit configuration wins over the ambient
/// environment, which is what lets a test harness pin a scheme without writing a process wide variable
/// that every other test in the assembly would observe.
/// </summary>
internal static class TlsPolicy
{
    /// <summary>
    /// Resolves the scheme. <paramref name="explicitUseTls"/> is <c>ServerArgs.UseTls</c> and wins
    /// outright. IVY_TLS is consulted next, where an empty value counts as unset. Otherwise TLS is on
    /// for local development on Windows only, since containers and hosted environments terminate TLS at
    /// a proxy.
    /// </summary>
    internal static bool Resolve(bool? explicitUseTls, string? ivyTlsEnv, bool isContainer, bool hasPortEnv, bool isWindows)
    {
        if (explicitUseTls.HasValue)
            return explicitUseTls.Value;

        if (!string.IsNullOrEmpty(ivyTlsEnv))
            return ivyTlsEnv.ToLowerInvariant() is "1" or "true" or "yes" or "on";

        return !isContainer && !hasPortEnv && isWindows;
    }
}
