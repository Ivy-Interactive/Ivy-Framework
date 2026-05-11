using Ivy.Core.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Core.Auth;

public static class CookieRegistryExtensions
{

    public static IActionResult? WriteCookiesToResponse(
        this Controller controller,
        AppSessionStore sessionStore,
        CookieJarId cookieJarId,
        string intent,
        out CookieJar cookies,
        out bool triggerMachineAuthSync)
    {
        if (!sessionStore.TryRemoveCookies(cookieJarId, intent, out cookies, out triggerMachineAuthSync))
        {
            return controller.BadRequest("Invalid or expired cookie jar ID, or intent mismatch.");
        }

        cookies.WriteToResponse(controller.HttpContext.Response);
        return null;
    }

    public static CookieJarId RegisterAuthSessionCookies(
        this AppSessionStore sessionStore,
        IAuthSession authSession,
        IEnumerable<string>? brokeredProvidersToDelete = null,
        IEnumerable<string>? connectedProvidersToDelete = null,
        bool triggerMachineAuthSync = false)
    {
        var cookies = new CookieJar();
        cookies.AddCookiesForAuthToken(authSession.AuthToken, brokeredProvidersToDelete);
        cookies.AddCookiesForAuthSessionData(authSession.AuthSessionData);

        // Filter out brokered session providers that have been globally removed (if machineId is provided)
        IReadOnlyDictionary<string, IAuthTokenHandlerSession> sessionsToWrite = authSession.BrokeredSessions;
        HashSet<string>? removedProviders = brokeredProvidersToDelete != null
            ? new(brokeredProvidersToDelete)
            : null;

        if (removedProviders != null && removedProviders.Count > 0)
        {
            sessionsToWrite = authSession.BrokeredSessions
                .Where(kvp => !removedProviders.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        cookies.AddCookiesForBrokeredSessions(sessionsToWrite);

        // Add connected accounts
        cookies.AddCookiesForConnectedAccounts(authSession.ConnectedAccounts);

        // Delete cookies for disconnected connected account providers
        if (connectedProvidersToDelete != null)
        {
            var cookieOptions = CreateAuthCookieOptions();
            foreach (var provider in connectedProvidersToDelete)
            {
                cookies.Delete(PrefixCookieName($"conn_{provider}_access_token"), cookieOptions);
                cookies.Delete(PrefixCookieName($"conn_{provider}_refresh_token"), cookieOptions);
                cookies.Delete(PrefixCookieName($"conn_{provider}_auth_tag"), cookieOptions);
            }
        }

        // Also delete cookies for removed brokered providers
        if (removedProviders != null && removedProviders.Count > 0)
        {
            var cookieOptions = CreateAuthCookieOptions();
            foreach (var provider in removedProviders)
            {
                cookies.Delete(PrefixCookieName($"{provider}_access_token"), cookieOptions);
                cookies.Delete(PrefixCookieName($"{provider}_refresh_token"), cookieOptions);
                cookies.Delete(PrefixCookieName($"{provider}_auth_tag"), cookieOptions);
            }
        }

        return sessionStore.RegisterCookies(cookies, CookieJarIntents.SetAuthCookies, triggerMachineAuthSync);
    }

    public static CookieJarId RegisterAuthTokenCookies(this AppSessionStore sessionStore, AuthToken? authToken)
    {
        var cookies = new CookieJar();
        cookies.AddCookiesForAuthToken(authToken);
        return sessionStore.RegisterCookies(cookies, CookieJarIntents.SetAuthCookies);
    }

    public static CookieJarId RegisterAuthSessionDataCookies(this AppSessionStore sessionStore, string? authSessionData)
    {
        var cookies = new CookieJar();
        cookies.AddCookiesForAuthSessionData(authSessionData);
        return sessionStore.RegisterCookies(cookies, CookieJarIntents.SetAuthCookies);
    }

    public static void AddCookiesForAuthToken(this CookieJar cookies, AuthToken? authToken, IEnumerable<string>? sessionsToDelete = null)
    {
        var authTokenName = PrefixCookieName("access_token");
        var refreshTokenName = PrefixCookieName("refresh_token");
        var tagName = PrefixCookieName("auth_tag");

        if (string.IsNullOrEmpty(authToken?.AccessToken))
        {
            cookies.Delete(authTokenName, CreateAuthCookieOptions());
            cookies.Delete(refreshTokenName, CreateAuthCookieOptions());
            cookies.Delete(tagName, CreateAuthCookieOptions());

            // Delete brokered auth session cookies
            if (sessionsToDelete != null)
            {
                var cookieOptions = CreateAuthCookieOptions();
                foreach (var provider in sessionsToDelete)
                {
                    cookies.Delete(PrefixCookieName($"{provider}_access_token"), cookieOptions);
                    cookies.Delete(PrefixCookieName($"{provider}_refresh_token"), cookieOptions);
                    cookies.Delete(PrefixCookieName($"{provider}_auth_tag"), cookieOptions);
                }
            }
        }
        else
        {
            var cookieOptions = CreateAuthCookieOptions();

            cookies.Append(authTokenName, authToken.AccessToken, cookieOptions);

            if (!string.IsNullOrEmpty(authToken.RefreshToken))
            {
                cookies.Append(refreshTokenName, authToken.RefreshToken, cookieOptions);
            }
            else
            {
                cookies.Delete(refreshTokenName, CreateAuthCookieOptions());
            }

            if (authToken.Tag != null)
            {
                cookies.Append(tagName, authToken.Tag, cookieOptions);
            }
            else
            {
                cookies.Delete(tagName, CreateAuthCookieOptions());
            }
        }
    }

    public static void AddCookiesForAuthSessionData(this CookieJar cookies, string? authSessionData)
    {
        var authSessionDataName = PrefixCookieName("auth_session_data");

        var cookieOptions = CreateAuthCookieOptions(forceLaxSameSite: true);
        if (authSessionData == null)
        {
            cookies.Delete(authSessionDataName, cookieOptions);
        }
        else
        {
            cookies.Append(authSessionDataName, authSessionData, cookieOptions);
        }
    }

    public static void AddCookiesForConnectedAccounts(this CookieJar cookies, IReadOnlyDictionary<string, IAuthSession> connectedAccounts)
    {
        var cookieOptions = CreateAuthCookieOptions();

        foreach (var (provider, session) in connectedAccounts)
        {
            var accessTokenName = PrefixCookieName($"conn_{provider}_access_token");
            var refreshTokenName = PrefixCookieName($"conn_{provider}_refresh_token");
            var tagName = PrefixCookieName($"conn_{provider}_auth_tag");

            if (!string.IsNullOrEmpty(session.AuthToken?.AccessToken))
            {
                cookies.Append(accessTokenName, session.AuthToken.AccessToken, cookieOptions);
            }
            else
            {
                cookies.Delete(accessTokenName, CreateAuthCookieOptions());
            }

            if (!string.IsNullOrEmpty(session.AuthToken?.RefreshToken))
            {
                cookies.Append(refreshTokenName, session.AuthToken.RefreshToken, cookieOptions);
            }
            else
            {
                cookies.Delete(refreshTokenName, CreateAuthCookieOptions());
            }

            if (session.AuthToken?.Tag != null)
            {
                cookies.Append(tagName, session.AuthToken.Tag, cookieOptions);
            }
            else
            {
                cookies.Delete(tagName, CreateAuthCookieOptions());
            }
        }
    }

    public static void AddCookiesForBrokeredSessions(this CookieJar cookies, IReadOnlyDictionary<string, IAuthTokenHandlerSession> brokeredSessions)
    {
        var cookieOptions = CreateAuthCookieOptions();

        foreach (var (provider, session) in brokeredSessions)
        {
            var accessTokenName = PrefixCookieName($"{provider}_access_token");
            var refreshTokenName = PrefixCookieName($"{provider}_refresh_token");
            var tagName = PrefixCookieName($"{provider}_auth_tag");

            // Store access token
            if (!string.IsNullOrEmpty(session.AuthToken?.AccessToken))
            {
                cookies.Append(accessTokenName, session.AuthToken.AccessToken, cookieOptions);
            }
            else
            {
                cookies.Delete(accessTokenName, CreateAuthCookieOptions());
            }

            // Store refresh token if present
            if (!string.IsNullOrEmpty(session.AuthToken?.RefreshToken))
            {
                cookies.Append(refreshTokenName, session.AuthToken.RefreshToken, cookieOptions);
            }
            else
            {
                cookies.Delete(refreshTokenName, CreateAuthCookieOptions());
            }

            // Store tag if present
            if (session.AuthToken?.Tag != null)
            {
                cookies.Append(tagName, session.AuthToken.Tag, cookieOptions);
            }
            else
            {
                cookies.Delete(tagName, CreateAuthCookieOptions());
            }
        }
    }

    internal static string PrefixCookieName(string name)
    {
        var prefix = global::Ivy.Server.AuthCookiePrefix;
        return string.IsNullOrEmpty(prefix) ? name : $"{prefix}__{name}";
    }

    private static CookieOptions CreateAuthCookieOptions(bool forceLaxSameSite = false)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = forceLaxSameSite ? SameSiteMode.Lax : SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            Path = "/",
        };

        // Apply custom configuration if provided
        global::Ivy.Server.ConfigureAuthCookieOptions?.Invoke(cookieOptions);

        return cookieOptions;
    }
}
