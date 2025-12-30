using System.Collections.Concurrent;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ivy.Core.HttpTunneling;

public class HttpTunnelResponse
{
    public string RequestId { get; set; } = null!;
    public int StatusCode { get; set; }
    public Dictionary<string, string[]>? Headers { get; set; }
    public string? Body { get; set; }
    public string? ContentType { get; set; }
    public string? ErrorMessage { get; set; }
}

public class HttpTunnelingController : Controller
{
    private static readonly ConcurrentDictionary<string, PendingHttpRequest> _pendingRequests = new();

    public static IDisposable Register(string requestId, PendingHttpRequest pendingRequest)
    {
        if (!_pendingRequests.TryAdd(requestId, pendingRequest))
        {
            throw new InvalidOperationException($"Request {requestId} is already registered");
        }

        return new RequestUnsubscriber(requestId);
    }

    public static void CancelRequestsForConnection(string connectionId, string reason)
    {
        var exception = new Exception($"HTTP tunnel request cancelled: {reason}");

        foreach (var kvp in _pendingRequests.Where(kvp => kvp.Key.StartsWith($"{connectionId}:")))
        {
            kvp.Value.CompletionSource.TrySetException(exception);
            _pendingRequests.TryRemove(kvp.Key, out _);
        }
    }

    [Route("ivy/http-tunnel/response")]
    [HttpPost]
    public IActionResult HttpResponse(
        [FromBody] HttpTunnelResponse response,
        [FromHeader(Name = "X-Connection-Id")] string connectionId,
        [FromServices] ILogger<HttpTunnelingController> logger)
    {
        if (string.IsNullOrEmpty(connectionId))
        {
            logger.LogWarning("HttpResponse: Missing X-Connection-Id header");
            return BadRequest("Missing X-Connection-Id header");
        }

        logger.LogDebug("HttpResponse: {RequestId} with status {StatusCode} for connection {ConnectionId}",
            response.RequestId, response.StatusCode, connectionId);

        var fullRequestId = $"{connectionId}:{response.RequestId}";

        if (!_pendingRequests.TryRemove(fullRequestId, out var pendingRequest))
        {
            logger.LogWarning("HttpResponse: Request {RequestId} not found or already completed", response.RequestId);
            return NotFound("Request not found");
        }

        try
        {
            var responseMessage = BuildResponseMessage(response);
            pendingRequest.CompletionSource.TrySetResult(responseMessage);
        }
        catch (Exception ex)
        {
            pendingRequest.CompletionSource.TrySetException(ex);
        }

        logger.LogDebug("HttpResponse: Successfully completed request {RequestId}", response.RequestId);
        return Ok();
    }

    private static HttpResponseMessage BuildResponseMessage(HttpTunnelResponse response)
    {
        if (response.StatusCode == 0)
        {
            throw new HttpRequestException(
                response.ErrorMessage ?? "Network error occurred during tunneled HTTP request");
        }

        var responseMessage = new HttpResponseMessage((System.Net.HttpStatusCode)response.StatusCode);

        if (response.Headers != null)
        {
            foreach (var header in response.Headers)
            {
                responseMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        if (!string.IsNullOrEmpty(response.Body))
        {
            var bodyBytes = Convert.FromBase64String(response.Body);
            var content = new ByteArrayContent(bodyBytes);

            if (!string.IsNullOrEmpty(response.ContentType))
            {
                content.Headers.ContentType = MediaTypeHeaderValue.Parse(response.ContentType);
            }

            if (response.Headers != null)
            {
                foreach (var header in response.Headers)
                {
                    content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            responseMessage.Content = content;
        }

        return responseMessage;
    }

    private sealed class RequestUnsubscriber(string requestId) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_pendingRequests.TryRemove(requestId, out var pendingRequest))
                {
                    pendingRequest.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
