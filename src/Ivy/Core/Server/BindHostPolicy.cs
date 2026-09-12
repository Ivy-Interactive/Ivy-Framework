namespace Ivy.Core.Server;

/// <summary>
/// The process facts the bind decision reads. <see cref="FromProcess"/> is the only place these
/// three environment variables are read, so a test constructs the record instead of mutating them.
/// </summary>
internal readonly record struct BindEnvironment(bool IsContainer, bool HasPortEnv, string? IvyTls, bool IsWindows)
{
    internal static BindEnvironment FromProcess() => new(
        Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true",
        Environment.GetEnvironmentVariable("PORT") != null,
        Environment.GetEnvironmentVariable("IVY_TLS"),
        OperatingSystem.IsWindows());
}

/// <summary>
/// The address Kestrel binds. <see cref="Host"/> is the address actually bound, which is what the
/// CORS and host filtering defaults have to key off (a CLI-only command binds loopback below
/// whatever host was requested).
/// </summary>
internal readonly record struct BindAddress(string Scheme, string Host, int Port)
{
    internal string Url => $"{Scheme}://{Host}:{Port}";
}

internal static class BindHostPolicy
{
    /// <summary>
    /// Resolves the scheme, host and port to bind. Bind loopback for local dev (avoids a Windows
    /// Firewall prompt), wildcard in containers so health probes can reach the app; hosted
    /// environments such as Sliplane set PORT and need 0.0.0.0. An explicit host always wins.
    /// </summary>
    internal static BindAddress Resolve(BindEnvironment environment, string? argsHost, int argsPort, bool isCliCommand, bool? argsUseTls)
    {
        // A CLI-only command needs DI but never calls app.StartAsync(), so it binds loopback on
        // port 0 whatever the requested host is, and never negotiates TLS.
        if (isCliCommand)
            return new BindAddress("http", "localhost", 0);

        var host = argsHost ?? (environment.IsContainer || environment.HasPortEnv ? "*" : "localhost");

        return new BindAddress(UseTls(environment, argsUseTls) ? "https" : "http", host, argsPort);
    }

    /// <summary>
    /// True when the server should negotiate TLS. Explicit configuration wins over IVY_TLS, which
    /// decides when set; otherwise the default is TLS for local dev on Windows only.
    /// </summary>
    internal static bool UseTls(BindEnvironment environment, bool? argsUseTls)
    {
        if (argsUseTls.HasValue)
            return argsUseTls.Value;

        return TlsPolicy.IsEnabled(environment.IvyTls, fallback: !environment.IsContainer && !environment.HasPortEnv && environment.IsWindows);
    }
}
