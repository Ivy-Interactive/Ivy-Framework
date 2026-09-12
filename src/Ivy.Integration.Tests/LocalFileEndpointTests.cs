using System.Net;

namespace Ivy.Integration.Tests;

/// <summary>
/// End-to-end reads through <c>GET /ivy/local-file</c> with roots configured on the server.
/// </summary>
public class LocalFileEndpointTests : IAsyncLifetime
{
    private string _root = null!;
    private string _outside = null!;
    private string _baseDirectory = null!;

    public Task InitializeAsync()
    {
        _baseDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _root = Directory.CreateDirectory(Path.Combine(_baseDirectory, "root")).FullName;
        _outside = Directory.CreateDirectory(Path.Combine(_baseDirectory, "outside")).FullName;
        File.WriteAllText(Path.Combine(_root, "photo.png"), "Fake PNG content");
        File.WriteAllText(Path.Combine(_outside, "secret.txt"), "Secret content");
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        if (Directory.Exists(_baseDirectory))
        {
            Directory.Delete(_baseDirectory, true);
        }

        return Task.CompletedTask;
    }

    [Fact]
    public async Task WithConfiguredRoot_ServesFileInsideItAndRejectsEverythingElse()
    {
        await using var server = await IvyTestServer.CreateAsync(s => s.DangerouslyAllowLocalFiles(_root));
        using var client = new HttpClient { BaseAddress = new Uri(server.BaseUrl) };

        var inside = await client.GetAsync($"/ivy/local-file?path={Uri.EscapeDataString(Path.Combine(_root, "photo.png"))}");
        Assert.Equal(HttpStatusCode.OK, inside.StatusCode);
        Assert.Equal("image/png", inside.Content.Headers.ContentType?.MediaType);
        Assert.Equal("Fake PNG content", await inside.Content.ReadAsStringAsync());

        var outside = await client.GetAsync($"/ivy/local-file?path={Uri.EscapeDataString(Path.Combine(_outside, "secret.txt"))}");
        Assert.Equal(HttpStatusCode.NotFound, outside.StatusCode);

        var traversal = Path.Combine(_root, "..", "outside", "secret.txt");
        var escaped = await client.GetAsync($"/ivy/local-file?path={Uri.EscapeDataString(traversal)}");
        Assert.Equal(HttpStatusCode.NotFound, escaped.StatusCode);
    }

    [Fact]
    public async Task WithoutRoots_StillServesAnyReadableFile()
    {
        // The no-argument overload is deliberately unconfined; this is the compatibility assertion.
        await using var server = await IvyTestServer.CreateAsync(s => s.DangerouslyAllowLocalFiles());
        using var client = new HttpClient { BaseAddress = new Uri(server.BaseUrl) };

        var response = await client.GetAsync($"/ivy/local-file?path={Uri.EscapeDataString(Path.Combine(_outside, "secret.txt"))}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Secret content", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task WithExtensionAllowlist_RejectsUnlistedExtensions()
    {
        await using var server = await IvyTestServer.CreateAsync(s =>
            s.DangerouslyAllowLocalFiles(_root, _outside).AllowLocalFileExtensions("png"));
        using var client = new HttpClient { BaseAddress = new Uri(server.BaseUrl) };

        var allowed = await client.GetAsync($"/ivy/local-file?path={Uri.EscapeDataString(Path.Combine(_root, "photo.png"))}");
        Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);

        var rejected = await client.GetAsync($"/ivy/local-file?path={Uri.EscapeDataString(Path.Combine(_outside, "secret.txt"))}");
        Assert.Equal(HttpStatusCode.NotFound, rejected.StatusCode);
    }

    [Fact]
    public async Task WithoutOptIn_ReturnsNotFound()
    {
        await using var server = await IvyTestServer.CreateAsync();
        using var client = new HttpClient { BaseAddress = new Uri(server.BaseUrl) };

        var response = await client.GetAsync($"/ivy/local-file?path={Uri.EscapeDataString(Path.Combine(_root, "photo.png"))}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
