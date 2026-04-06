namespace Ivy.Integration.Tests;

public class IvyTestFixture : IAsyncLifetime
{
    private IvyTestServer? _server;
    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _server = await IvyTestServer.CreateAsync();
        Client = new HttpClient { BaseAddress = new Uri(_server.BaseUrl) };
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        if (_server != null) await _server.DisposeAsync();
    }
}
