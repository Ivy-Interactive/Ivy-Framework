using Ivy.Apps;
using Ivy.Client;
using Ivy.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ivy.Core.Helpers;

namespace Ivy.Auth;

public record SetAuthCookiesRequest(string CookieJarId, string? ConnectionId, bool TriggerMachineReload);

public class AuthController() : Controller
{
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
