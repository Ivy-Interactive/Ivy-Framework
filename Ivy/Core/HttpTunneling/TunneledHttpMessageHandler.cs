namespace Ivy.Core.HttpTunneling;

public class HttpTunnelRequest
{
    public string RequestId { get; set; } = null!;
    public string Method { get; set; } = null!;
    public string Url { get; set; } = null!;
    public Dictionary<string, string[]>? Headers { get; set; }
    public string? Body { get; set; }
    public string? ContentType { get; set; }
}

public class TunneledHttpMessageHandler : HttpMessageHandler
{
    private readonly IClientProvider _clientProvider;
    private readonly string _connectionId;

    public TunneledHttpMessageHandler(IClientProvider clientProvider, string connectionId)
    {
        _clientProvider = clientProvider ?? throw new ArgumentNullException(nameof(clientProvider));
        _connectionId = connectionId ?? throw new ArgumentNullException(nameof(connectionId));
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Generate unique request ID
        var requestGuid = Guid.NewGuid().ToString();
        var requestId = $"{_connectionId}:{requestGuid}";

        var pendingRequest = new PendingHttpRequest(cancellationToken);

        using var registration = HttpTunnelingController.Register(requestId, pendingRequest);

        var tunnelRequest = await BuildRequestAsync(requestGuid, request, cancellationToken);

        _clientProvider.Sender.Send("HttpRequest", tunnelRequest);

        return await pendingRequest.CompletionSource.Task;
    }

    private async Task<HttpTunnelRequest> BuildRequestAsync(
        string requestId,
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var tunnelRequest = new HttpTunnelRequest
        {
            RequestId = requestId,
            Method = request.Method.Method,
            Url = request.RequestUri?.ToString() ?? throw new InvalidOperationException("Request URI is null"),
        };

        var headers = new Dictionary<string, string[]>();
        foreach (var header in request.Headers)
        {
            headers[header.Key] = header.Value.ToArray();
        }
        if (headers.Count > 0)
        {
            tunnelRequest.Headers = headers;
        }

        if (request.Content != null)
        {
            foreach (var header in request.Content.Headers)
            {
                headers[header.Key] = header.Value.ToArray();
            }

            if (request.Content.Headers.ContentType != null)
            {
                tunnelRequest.ContentType = request.Content.Headers.ContentType.ToString();
            }

            var contentBytes = await request.Content.ReadAsByteArrayAsync(cancellationToken);
            if (contentBytes.Length > 0)
            {
                tunnelRequest.Body = Convert.ToBase64String(contentBytes);
            }
        }

        return tunnelRequest;
    }

}
