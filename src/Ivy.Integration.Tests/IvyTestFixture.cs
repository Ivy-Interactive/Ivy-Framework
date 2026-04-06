using Microsoft.AspNetCore.Builder;

namespace Ivy.Integration.Tests;

public class IvyTestFixture : IAsyncLifetime
{
    private WebApplication? _app;
    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var server = new Server(new ServerArgs
        {
            Port = 0,
            Silent = true,
            Host = "127.0.0.1"
        });
        _app = server.BuildWebApplication();
        if (_app == null)
            throw new InvalidOperationException("Failed to build WebApplication");
        await _app.StartAsync();
        Client = new HttpClient { BaseAddress = new Uri(_app.Urls.First()) };
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        if (_app != null) await _app.StopAsync();
    }
}
