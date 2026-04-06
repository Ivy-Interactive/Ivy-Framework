namespace Ivy.Integration.Tests;

public class UploadEndpointTests : IClassFixture<IvyTestFixture>
{
    private readonly HttpClient _client;

    public UploadEndpointTests(IvyTestFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task Upload_WithInvalidIds_ReturnsError()
    {
        var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(new byte[] { 1, 2, 3 }), "file", "test.txt");

        var response = await _client.PostAsync("/ivy/upload/invalid-connection-id/invalid-upload-id", content);

        // Should return an error status for invalid connection/upload IDs
        Assert.True(
            (int)response.StatusCode >= 400,
            $"Expected error status but got {(int)response.StatusCode}");
    }
}
