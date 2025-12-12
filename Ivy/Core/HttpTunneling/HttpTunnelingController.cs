using System.Collections.Concurrent;
using Ivy.Apps;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ivy.Core.HttpTunneling;

public class HttpTunnelingController : Controller
{
    // Track tunnel handlers by connection ID so they can be accessed during OnConnectedAsync
    internal static readonly ConcurrentDictionary<string, IHttpTunnelRequestManager> TunnelHandlers = new();

    [Route("ivy/http-tunnel/response")]
    [HttpPost]
    public IActionResult HttpResponse(
        [FromBody] HttpTunnelResponseDto response,
        [FromHeader(Name = "X-Connection-Id")] string connectionId,
        [FromServices] AppSessionStore sessionStore,
        [FromServices] ILogger<HttpTunnelingController> logger)
    {
        if (string.IsNullOrEmpty(connectionId))
        {
            logger.LogWarning("HttpResponse: Missing X-Connection-Id header");
            return BadRequest("Missing X-Connection-Id header");
        }

        logger.LogDebug("HttpResponse: {RequestId} with status {StatusCode} for connection {ConnectionId}",
            response.RequestId, response.StatusCode, connectionId);

        // Try to get the tunnel handler from the session store first
        IHttpTunnelRequestManager? requestManager = null;
        if (sessionStore.Sessions.TryGetValue(connectionId, out var appSession))
        {
            requestManager = appSession.AppServices.GetService<IHttpTunnelRequestManager>();
        }

        // Fall back to the static dictionary if not in session store yet (during OnConnectedAsync)
        requestManager ??= TunnelHandlers.TryGetValue(connectionId, out var handler) ? handler : null;

        if (requestManager == null)
        {
            logger.LogWarning("HttpResponse: No IHttpTunnelRequestManager found for connection {ConnectionId}", connectionId);
            return NotFound("Connection not found");
        }

        if (!requestManager.TryCompleteRequest(response.RequestId, response))
        {
            logger.LogWarning("HttpResponse: Request {RequestId} not found or already completed", response.RequestId);
            return NotFound("Request not found");
        }

        logger.LogDebug("HttpResponse: Successfully completed request {RequestId}", response.RequestId);
        return Ok();
    }
}
