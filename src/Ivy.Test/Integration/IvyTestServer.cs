using Ivy.Core.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR.Client;

namespace Ivy.Test.Integration;

public class IvyTestServer : IAsyncDisposable
{
    private readonly WebApplication _app;
    public AppSessionStore SessionStore { get; }
    public string BaseUrl { get; }

    private IvyTestServer(WebApplication app, AppSessionStore sessionStore, string baseUrl)
    {
        _app = app;
        SessionStore = sessionStore;
        BaseUrl = baseUrl;
    }

    public static async Task<IvyTestServer> CreateAsync()
    {
        var sessionStore = new AppSessionStore();
        var server = new Server(new ServerArgs { Port = 0, Silent = true, Host = "127.0.0.1" });
        server.AddApp(new AppDescriptor
        {
            Id = AppIds.Default,
            Title = "Test App",
            ViewFunc = _ => "Hello from test",
            Group = ["Apps"],
            IsVisible = true
        });

        var app = server.BuildWebApplication(sessionStore);
        if (app == null)
            throw new InvalidOperationException("Failed to build WebApplication");

        await app.StartAsync();
        var baseUrl = app.Urls.First();
        return new IvyTestServer(app, sessionStore, baseUrl);
    }

    public HubConnection CreateHubConnection()
    {
        return new HubConnectionBuilder()
            .WithUrl($"{BaseUrl}/ivy/messages?machineId=test-machine")
            .Build();
    }

    public async ValueTask DisposeAsync()
    {
        await _app.StopAsync();
        await _app.DisposeAsync();
        SessionStore.Dispose();
    }
}
