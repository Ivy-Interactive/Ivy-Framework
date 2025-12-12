using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text;

namespace Ivy.Core.HttpTunneling;

public class TunneledHttpMessageHandler : HttpMessageHandler, IHttpTunnelRequestManager
{
    private readonly IClientProvider _clientProvider;
    private readonly ConcurrentDictionary<string, PendingHttpRequest> _pendingRequests = new();

    public TunneledHttpMessageHandler(IClientProvider clientProvider)
    {
        _clientProvider = clientProvider ?? throw new ArgumentNullException(nameof(clientProvider));
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Generate unique request ID
        var requestId = Guid.NewGuid().ToString();

        // Create pending request with timeout and cancellation
        var pendingRequest = new PendingHttpRequest(cancellationToken);

        // Add to pending requests dictionary
        if (!_pendingRequests.TryAdd(requestId, pendingRequest))
        {
            pendingRequest.Dispose();
            throw new InvalidOperationException($"Failed to register request {requestId}");
        }

        try
        {
            var requestDto = await BuildRequestDtoAsync(requestId, request, cancellationToken);

            // Send to frontend via ClientProvider
            _clientProvider.Sender.Send("HttpRequest", requestDto);

            Console.WriteLine($"Sent tunneled HTTP request {requestId} to frontend: {System.Text.Json.JsonSerializer.Serialize(requestDto)}");

            // Await response
            var response = await pendingRequest.CompletionSource.Task;

            return response;
        }
        catch
        {
            // Clean up on error
            _pendingRequests.TryRemove(requestId, out _);
            throw;
        }
        finally
        {
            // Always dispose the pending request to clean up resources
            if (_pendingRequests.TryRemove(requestId, out var removedRequest))
            {
                removedRequest.Dispose();
            }
        }
    }

    private async Task<HttpTunnelRequestDto> BuildRequestDtoAsync(
        string requestId,
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var dto = new HttpTunnelRequestDto
        {
            RequestId = requestId,
            Method = request.Method.Method,
            Url = request.RequestUri?.ToString() ?? throw new InvalidOperationException("Request URI is null"),
        };

        // Convert headers
        var headers = new Dictionary<string, string[]>();
        foreach (var header in request.Headers)
        {
            headers[header.Key] = header.Value.ToArray();
        }
        if (headers.Count > 0)
        {
            dto.Headers = headers;
        }

        // Handle request content
        if (request.Content != null)
        {
            // Add content headers
            foreach (var header in request.Content.Headers)
            {
                headers[header.Key] = header.Value.ToArray();
            }

            // Set content type
            if (request.Content.Headers.ContentType != null)
            {
                dto.ContentType = request.Content.Headers.ContentType.ToString();
            }

            // Read and encode body as base64
            var contentBytes = await request.Content.ReadAsByteArrayAsync(cancellationToken);
            if (contentBytes.Length > 0)
            {
                dto.Body = Convert.ToBase64String(contentBytes);
            }
        }

        return dto;
    }

    private HttpResponseMessage BuildResponseMessage(HttpTunnelResponseDto response)
    {
        // Handle network errors
        if (response.StatusCode == 0)
        {
            throw new HttpRequestException(
                response.ErrorMessage ?? "Network error occurred during tunneled HTTP request");
        }

        var responseMessage = new HttpResponseMessage((System.Net.HttpStatusCode)response.StatusCode);

        // Set response headers
        if (response.Headers != null)
        {
            foreach (var header in response.Headers)
            {
                // Try to add to response headers first
                if (!responseMessage.Headers.TryAddWithoutValidation(header.Key, header.Value))
                {
                    // If it fails, it might be a content header, we'll add it after creating content
                }
            }
        }

        // Set response body
        if (!string.IsNullOrEmpty(response.Body))
        {
            var bodyBytes = Convert.FromBase64String(response.Body);
            var content = new ByteArrayContent(bodyBytes);

            // Set content type
            if (!string.IsNullOrEmpty(response.ContentType))
            {
                content.Headers.ContentType = MediaTypeHeaderValue.Parse(response.ContentType);
            }

            // Add content headers
            if (response.Headers != null)
            {
                foreach (var header in response.Headers)
                {
                    // Try to add headers that might be content headers
                    content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            responseMessage.Content = content;
        }

        return responseMessage;
    }

    public bool TryCompleteRequest(string requestId, HttpTunnelResponseDto response)
    {
        if (!_pendingRequests.TryGetValue(requestId, out var pendingRequest))
        {
            return false;
        }

        try
        {
            var responseMessage = BuildResponseMessage(response);
            pendingRequest.CompletionSource.TrySetResult(responseMessage);
            return true;
        }
        catch (Exception ex)
        {
            pendingRequest.CompletionSource.TrySetException(ex);
            return true;
        }
    }

    public void CancelAllPendingRequests(string reason)
    {
        var exception = new Exception($"HTTP tunnel request cancelled: {reason}");

        foreach (var kvp in _pendingRequests)
        {
            kvp.Value.CompletionSource.TrySetException(exception);
        }

        _pendingRequests.Clear();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            CancelAllPendingRequests("HttpMessageHandler disposed");
        }

        base.Dispose(disposing);
    }
}
