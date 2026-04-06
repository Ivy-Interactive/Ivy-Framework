using System.Net;

namespace Ivy.Test;

public class DictationTests
{
    [Fact]
    public void EnableDictation_SetsDictationProps()
    {
        var state = new MockState<string>("hello");
        var widget = state.ToTextInput();
        var result = widget.EnableDictation();

        Assert.True(result.Dictation);
        Assert.Null(result.DictationLanguage);
    }

    [Fact]
    public void EnableDictation_WithLanguage_SetsDictationLanguage()
    {
        var state = new MockState<string>("");
        var widget = state.ToTextInput();
        var result = widget.EnableDictation(language: "de-DE");

        Assert.True(result.Dictation);
        Assert.Equal("de-DE", result.DictationLanguage);
    }

    [Fact]
    public async Task AzureSpeechTranscriptionService_TranscribeAsync_Success()
    {
        var responseJson = """{"RecognitionStatus":"Success","DisplayText":"Hello world","Offset":0,"Duration":0}""";
        var handler = new MockHttpHandler(responseJson, HttpStatusCode.OK);
        var httpClient = new HttpClient(handler);
        var service = new AzureSpeechTranscriptionService("westeurope", "test-key", httpClient);

        using var audioStream = new MemoryStream(new byte[] { 0, 1, 2, 3 });
        var result = await service.TranscribeAsync(audioStream, "audio/webm", "en-US");

        Assert.Equal("Hello world", result);
        Assert.Contains("westeurope.stt.speech.microsoft.com", handler.LastRequestUri?.ToString());
        Assert.Contains("language=en-US", handler.LastRequestUri?.ToString());
        Assert.Equal("test-key", handler.LastRequest?.Headers.GetValues("Ocp-Apim-Subscription-Key").First());
    }

    [Fact]
    public async Task AzureSpeechTranscriptionService_TranscribeAsync_NoMatch_ReturnsEmpty()
    {
        var responseJson = """{"RecognitionStatus":"NoMatch","Offset":0,"Duration":0}""";
        var handler = new MockHttpHandler(responseJson, HttpStatusCode.OK);
        var httpClient = new HttpClient(handler);
        var service = new AzureSpeechTranscriptionService("westeurope", "test-key", httpClient);

        using var audioStream = new MemoryStream(new byte[] { 0, 1, 2, 3 });
        var result = await service.TranscribeAsync(audioStream, "audio/webm");

        Assert.Equal("", result);
    }

    [Fact]
    public async Task AzureSpeechTranscriptionService_TranscribeAsync_DefaultLanguage()
    {
        var responseJson = """{"RecognitionStatus":"Success","DisplayText":"Test"}""";
        var handler = new MockHttpHandler(responseJson, HttpStatusCode.OK);
        var httpClient = new HttpClient(handler);
        var service = new AzureSpeechTranscriptionService("eastus", "key123", httpClient);

        using var audioStream = new MemoryStream(new byte[] { 0 });
        await service.TranscribeAsync(audioStream, "audio/wav");

        Assert.Contains("language=en-US", handler.LastRequestUri?.ToString());
    }

    [Fact]
    public void AzureSpeechTranscriptionService_Constructor_ThrowsOnEmptyRegion()
    {
        Assert.Throws<ArgumentException>(() => new AzureSpeechTranscriptionService("", "key"));
    }

    [Fact]
    public void AzureSpeechTranscriptionService_Constructor_ThrowsOnEmptyKey()
    {
        Assert.Throws<ArgumentException>(() => new AzureSpeechTranscriptionService("region", ""));
    }

    private class MockHttpHandler(string responseContent, HttpStatusCode statusCode) : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }
        public Uri? LastRequestUri { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            LastRequestUri = request.RequestUri;
            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(responseContent)
            };
        }
    }
}
