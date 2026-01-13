using Ivy.Apps;
using Ivy.Client;
using Ivy.Core;
using Ivy.Hooks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ivy.Core.Helpers;

namespace Ivy.Auth;

public record SetAuthCookiesRequest(string CookieJarId, string? ConnectionId, bool TriggerMachineReload);

public class AuthController() : Controller
{
    [Route("ivy/auth/oauth-login")]
    [HttpGet]
    public async Task<IActionResult> OAuthLogin(
        [FromQuery] string optionId,
        [FromQuery] string callbackId,
        [FromQuery] string connectionId,
        [FromServices] AppSessionStore sessionStore,
        [FromServices] ILogger<AuthController> logger)
    {
        if (string.IsNullOrWhiteSpace(optionId) || string.IsNullOrWhiteSpace(callbackId) || string.IsNullOrWhiteSpace(connectionId))
        {
            logger.LogWarning("OAuth login failed: Missing required parameters");
            return BadRequest("Authentication error");
        }

        if (!sessionStore.Sessions.TryGetValue(connectionId, out var appSession))
        {
            logger.LogWarning("OAuth login failed: Session not found for connection {ConnectionId}", connectionId);
            return BadRequest("Authentication error");
        }

        var authService = appSession.AppServices.GetService<IAuthService>();
        if (authService == null)
        {
            logger.LogWarning("OAuth login failed: Auth service not configured for connection {ConnectionId}", connectionId);
            return BadRequest("Authentication error");
        }

        // Find the auth option by ID
        var options = authService.GetAuthOptions();
        var option = options.FirstOrDefault(o => o.Id == optionId);
        if (option == null)
        {
            logger.LogWarning("OAuth login failed: Auth option '{OptionId}' not found for connection {ConnectionId}", optionId, connectionId);
            return BadRequest("Authentication error");
        }

        // Construct the webhook endpoint
        var scheme = HttpContext.Request.Scheme;
        if (HttpContext.Request.Headers.TryGetValue("X-Forwarded-Proto", out var forwardedProto))
        {
            scheme = forwardedProto.ToString();
        }
        var host = HttpContext.Request.Host.Value ?? throw new InvalidOperationException("Host not found in request");
        var callback = new WebhookEndpoint(callbackId, scheme, host);

        try
        {
            // Get the OAuth URI and redirect to it
            var uri = await authService.GetOAuthUriAsync(option, callback, HttpContext.RequestAborted);
            return Redirect(uri.ToString());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "OAuth login failed: Error initiating OAuth for option '{OptionId}' on connection {ConnectionId}", optionId, connectionId);
            return BadRequest("Authentication error");
        }
    }

    [Route("ivy/auth/set-auth-cookies")]
    [HttpPatch]
    public async Task<IActionResult> SetAuthCookies(
        [FromBody] SetAuthCookiesRequest request,
        [FromServices] AppSessionStore sessionStore,
        [FromServices] IContentBuilder contentBuilder,
        [FromServices] ILogger<AuthController> logger)
    {
        if (this.WriteCookiesToResponse(
            sessionStore,
            new CookieJarId(request.CookieJarId),
            CookieJarIntents.SetAuthCookies,
            out var cookies) is { } errorResponse)
        {
            return errorResponse;
        }

        if (request.TriggerMachineReload)
        {
            if (cookies.TryGet("auth_token", out var authTokenValue) && !string.IsNullOrEmpty(authTokenValue))
            {
                // Trigger reload for all sessions with the same machineId on login
                if (HttpContext.Request.Headers.TryGetValue("X-Machine-Id", out var loginHeaderValue))
                {
                    var machineId = loginHeaderValue.ToString();
                    TriggerMachineReload(sessionStore, machineId, request.ConnectionId);
                }
            }
            else
            {
                // Trigger logout for all sessions with the same machineId on logout
                if (HttpContext.Request.Headers.TryGetValue("X-Machine-Id", out var headerValue))
                {
                    var machineId = headerValue.ToString();
                    await TriggerMachineLogout(sessionStore, machineId, request.ConnectionId, contentBuilder, logger);
                }
            }
        }

        return Ok();
    }

    private static string FindRootAncestor(AppSessionStore sessionStore, string connectionId)
    {
        var current = connectionId;
        while (sessionStore.Sessions.TryGetValue(current, out var session) && session.ParentId != null)
        {
            current = session.ParentId;
        }
        return current;
    }

    private static IEnumerable<AppSession> GetMachineSessions(
        AppSessionStore sessionStore,
        string machineId,
        string? excludeConnectionId)
    {
        var processedRoots = new HashSet<string>();
        if (!string.IsNullOrEmpty(excludeConnectionId))
        {
            var excludedRoot = FindRootAncestor(sessionStore, excludeConnectionId);
            processedRoots.Add(excludedRoot);
        }

        // Find all sessions with this machineId
        var allSessions = sessionStore.Sessions.Values
            .Where(s => !s.IsDisposed() && s.MachineId == machineId)
            .ToList();

        foreach (var session in allSessions)
        {
            // Find root for this session
            var sessionRoot = FindRootAncestor(sessionStore, session.ConnectionId);

            // Skip if we've already processed this root (includes the excluded root)
            if (!processedRoots.Add(sessionRoot))
            {
                continue;
            }

            yield return session;
        }
    }

    private static void TriggerMachineReload(
        AppSessionStore sessionStore,
        string machineId,
        string? excludeConnectionId)
    {
        foreach (var session in GetMachineSessions(sessionStore, machineId, excludeConnectionId))
        {
            // Just trigger page reload to pick up new auth cookies
            var clientProvider = session.AppServices.GetRequiredService<IClientProvider>();
            clientProvider.ReloadPage();
        }
    }

    private static async Task TriggerMachineLogout(
        AppSessionStore sessionStore,
        string machineId,
        string? excludeConnectionId,
        IContentBuilder contentBuilder,
        ILogger logger)
    {
        foreach (var session in GetMachineSessions(sessionStore, machineId, excludeConnectionId))
        {
            await SessionHelpers.AbandonSessionAsync(sessionStore, session, contentBuilder, resetTokenAndReload: true, triggerMachineReload: false, logger, "TriggerMachineLogout");
        }
    }
}
