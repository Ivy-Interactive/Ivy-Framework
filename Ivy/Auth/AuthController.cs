using System.Net;
using System.Text.Json;
using Ivy.Apps;
using Ivy.Client;
using Ivy.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ivy.Auth;

public record SetAuthTokenRequest(string TokenId, string? ConnectionId, bool TriggerRecursiveReload);

public class AuthController() : Controller
{
    [Route("ivy/auth/set-auth-token")]
    [HttpPatch]
    public async Task<IActionResult> SetAuthToken(
        [FromBody] SetAuthTokenRequest request,
        [FromServices] AppSessionStore sessionStore,
        [FromServices] IContentBuilder contentBuilder,
        [FromServices] ILogger<AuthController> logger)
    {
        if (!AuthTokenRegistry.TryRemove(request.TokenId, out var token))
        {
            return BadRequest("Invalid or expired token id.");
        }

        var cookies = HttpContext.Response.Cookies;
        if (string.IsNullOrEmpty(token?.AccessToken))
        {
            cookies.Delete("auth_token");
            cookies.Delete("auth_ext_refresh_token");

            // Trigger logout for all sessions with the same machineId
            if (HttpContext.Request.Headers.TryGetValue("X-Machine-Id", out var headerValue))
            {
                var machineId = headerValue.ToString();
                if (request.TriggerRecursiveReload)
                {
                    await TriggerMachineLogout(sessionStore, machineId, request.ConnectionId, contentBuilder, logger);
                }
            }
        }
        else
        {
            var isProduction = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = isProduction, // Enable Secure flag in production
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddYears(1),
            };

            var tokenJson = JsonSerializer.Serialize(token);

            // Calculate url-encoded token length
            var tokenJsonLength = WebUtility.UrlEncode(tokenJson).Length;
            var refreshTokenLength = token.RefreshToken != null
                ? WebUtility.UrlEncode(token.RefreshToken).Length
                : 0;

            // If the token is too big, try putting the refresh token into its own cookie.
            // I'm not trying to be overly precise here.
            const int CookieSizeLimit = 4000;

            if (tokenJsonLength > CookieSizeLimit && tokenJsonLength - refreshTokenLength < CookieSizeLimit)
            {
                var refreshToken = token.RefreshToken!; // non-nullness implied by condition above
                var modifiedToken = token with { RefreshToken = null };
                tokenJson = JsonSerializer.Serialize(modifiedToken);
                cookies.Append("auth_ext_refresh_token", refreshToken, cookieOptions);
            }
            else
            {
                cookies.Delete("auth_ext_refresh_token");
            }
            cookies.Append("auth_token", tokenJson, cookieOptions);

            // Trigger reload for all sessions with the same machineId on login
            if (HttpContext.Request.Headers.TryGetValue("X-Machine-Id", out var loginHeaderValue))
            {
                var machineId = loginHeaderValue.ToString();
                if (request.TriggerRecursiveReload)
                {
                    await TriggerMachineReload(sessionStore, machineId, request.ConnectionId);
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

    private static Task TriggerMachineReload(
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

            // Just trigger page reload to pick up new auth cookies
            var clientProvider = session.AppServices.GetRequiredService<IClientProvider>();
            clientProvider.ReloadPage();
        }

        return Task.CompletedTask;
    }

    private static async Task TriggerMachineLogout(
        AppSessionStore sessionStore,
        string machineId,
        string? excludeConnectionId,
        IContentBuilder contentBuilder,
        ILogger logger)
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

            await SessionHelpers.AbandonSessionAsync(session, contentBuilder, resetTokenAndReload: true, triggerRecursiveReload: false, logger, "TriggerMachineLogout");
        }
    }
}
