using System.Net;

namespace Ivy.Test.TestHelpers;

internal class MockHttpHandler(string responseContent, HttpStatusCode statusCode) : HttpMessageHandler
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
