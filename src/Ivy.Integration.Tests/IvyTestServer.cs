using Ivy.Core.Server;
using Microsoft.AspNetCore.SignalR.Client;

namespace Ivy.Integration.Tests;

public class IvyTestServer : IAsyncDisposable
{
    private readonly Microsoft.AspNetCore.Builder.WebApplication _app;
    private readonly AppSessionStore _sessionStore;

    public string BaseUrl { get; }
    public AppSessionStore SessionStore => _sessionStore;

    private IvyTestServer(Microsoft.AspNetCore.Builder.WebApplication app, AppSessionStore sessionStore, string baseUrl)
    {
        _app = app;
        _sessionStore = sessionStore;
        BaseUrl = baseUrl;
    }

    /// <param name="configure">
    /// Optional hook applied to the <see cref="Server"/> before the web application is built, for
    /// tests that need builder methods such as <c>AllowHosts</c> or <c>DangerouslyAllowLocalFiles</c>.
    /// </param>
    public static async Task<IvyTestServer> CreateAsync(Action<Server>? configure = null)
    {
        // Pin to HTTP for CI: Windows runners may not have a trusted ASP.NET Core dev certificate. Set per
        // server rather than through IVY_TLS, which the whole process, and so every parallel test, observes.
        var sessionStore = new AppSessionStore();
        var server = new Server(new ServerArgs { Port = 0, Silent = true, Host = "127.0.0.1", UseTls = false });
        server.AddApp(new AppDescriptor
        {
            Id = "test-app",
            Title = "Test App",
            ViewFunc = ctx => "Hello Integration Test",
            Group = [],
            IsVisible = true
        });

        configure?.Invoke(server);

        var app = server.BuildWebApplication(sessionStore)!;
        await app.StartAsync();

        var baseUrl = app.Urls.First();
        return new IvyTestServer(app, sessionStore, baseUrl);
    }

    public HubConnection CreateHubConnection()
    {
        return new HubConnectionBuilder()
            .WithUrl($"{BaseUrl}/ivy/messages?appId=test-app&machineId=test-machine&shell=false")
            .Build();
    }

    public async ValueTask DisposeAsync()
    {
        await _app.StopAsync();
        await _app.DisposeAsync();
        _sessionStore.Dispose();
    }
}
