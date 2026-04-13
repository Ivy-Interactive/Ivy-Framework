using System.Net;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class VersionCheckServiceTests
{
    [Fact]
    public void CurrentVersion_ReturnsAssemblyVersion()
    {
        var version = VersionCheckService.GetCurrentVersion();
        Assert.NotNull(version);
        Assert.NotEqual("0.0.0", version);
        Assert.True(Version.TryParse(version, out _));
    }

    [Fact]
    public async Task CheckForUpdates_WithNewerVersion_ReturnsHasUpdateTrue()
    {
        var factory = new FakeHttpClientFactory("""{"versions":["0.0.1","99.0.0"]}""");
        var service = new VersionCheckService(factory);

        var result = await service.CheckForUpdatesAsync();

        Assert.True(result.HasUpdate);
        Assert.Equal("99.0.0", result.LatestVersion);
        Assert.NotNull(result.LastChecked);
    }

    [Fact]
    public async Task CheckForUpdates_WithSameVersion_ReturnsHasUpdateFalse()
    {
        var currentVersion = VersionCheckService.GetCurrentVersion();
        var factory = new FakeHttpClientFactory($$"""
            {"versions":["{{currentVersion}}"]}
            """);
        var service = new VersionCheckService(factory);

        var result = await service.CheckForUpdatesAsync();

        Assert.False(result.HasUpdate);
        Assert.Equal(currentVersion, result.LatestVersion);
    }

    [Fact]
    public async Task CheckForUpdates_WithCachedResult_DoesNotCallApi()
    {
        var factory = new FakeHttpClientFactory("""{"versions":["99.0.0"]}""");
        var service = new VersionCheckService(factory);

        await service.CheckForUpdatesAsync();
        await service.CheckForUpdatesAsync();

        Assert.Equal(1, factory.CallCount);
    }

    [Fact]
    public async Task CheckForUpdates_WithNetworkError_ReturnsNullLatestVersion()
    {
        var factory = new FakeHttpClientFactory(statusCode: HttpStatusCode.InternalServerError);
        var service = new VersionCheckService(factory);

        var result = await service.CheckForUpdatesAsync();

        Assert.Null(result.LatestVersion);
        Assert.False(result.HasUpdate);
    }

    [Fact]
    public async Task CheckForUpdates_SkipsPreReleaseVersions()
    {
        var factory = new FakeHttpClientFactory("""{"versions":["1.0.0","99.0.0-pre-20260101"]}""");
        var service = new VersionCheckService(factory);

        var result = await service.CheckForUpdatesAsync();

        Assert.Equal("1.0.0", result.LatestVersion);
    }

    private class FakeHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpMessageHandler _handler;
        public int CallCount { get; private set; }

        public FakeHttpClientFactory(string? responseBody = null, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _handler = new FakeHandler(this, responseBody ?? "", statusCode);
        }

        public HttpClient CreateClient(string name) => new(_handler, disposeHandler: false);

        private class FakeHandler(FakeHttpClientFactory owner, string body, HttpStatusCode statusCode) : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                owner.CallCount++;
                return Task.FromResult(new HttpResponseMessage(statusCode)
                {
                    Content = new StringContent(body)
                });
            }
        }
    }
}
