namespace Ivy.Integration.Tests;

public class TestServerSchemeTests
{
    [Fact]
    public async Task TestServerUsesHttp()
    {
        await using var server = await IvyTestServer.CreateAsync();
        Assert.StartsWith("http://", server.BaseUrl);
    }
}
