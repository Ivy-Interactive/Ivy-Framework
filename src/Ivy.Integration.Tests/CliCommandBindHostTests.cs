using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.HostFiltering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Ivy.Integration.Tests;

/// <summary>
/// A CLI-only command binds loopback whatever the requested host is, so the CORS and host filtering
/// defaults have to be derived from the address actually bound. <c>Host = "*"</c> plus
/// <c>Describe = true</c> reproduces the container and PORT cases without touching an environment
/// variable, which matters because xUnit runs test classes in parallel.
/// </summary>
public class CliCommandBindHostTests
{
    [Fact]
    public async Task CliCommand_WithWildcardHost_ValidatesTheLoopbackHostNames()
    {
        await using var app = BuildCliApplication();

        var hostFiltering = app.Services.GetRequiredService<IOptions<HostFilteringOptions>>().Value;

        Assert.Equal(new[] { "localhost", "127.0.0.1", "[::1]" }, hostFiltering.AllowedHosts);
    }

    [Fact]
    public async Task CliCommand_WithWildcardHost_ReflectsLoopbackOriginsOnly()
    {
        await using var app = BuildCliApplication();

        var corsOptions = app.Services.GetRequiredService<IOptions<CorsOptions>>().Value;
        var policy = corsOptions.GetPolicy(corsOptions.DefaultPolicyName);

        Assert.NotNull(policy);
        Assert.True(policy.IsOriginAllowed("http://localhost:5173"));
        Assert.False(policy.IsOriginAllowed("https://evil.example"));
    }

    /// <summary>
    /// Builds the application without starting it: the CLI branch binds port 0 and only the DI
    /// container is under test here.
    /// </summary>
    private static WebApplication BuildCliApplication()
    {
        var server = new Server(new ServerArgs
        {
            Port = 0,
            Silent = true,
            Host = "*",
            Describe = true
        });

        var app = server.BuildWebApplication();
        Assert.NotNull(app);

        return app;
    }
}
