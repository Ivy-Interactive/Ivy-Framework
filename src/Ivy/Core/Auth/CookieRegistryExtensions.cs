using System.Net;
using System.Text.Json;
using Ivy.Core.Helpers;
using Ivy.Core.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Core.Auth;

public static class CookieRegistryExtensions
{

    public static IActionResult? WriteCookiesToResponse(this Controller controller, AppSessionStore sessionStore, CookieJarId cookieJarId, string intent, out CookieJar cookies)
    {
        if (!sessionStore.TryRemoveCookies(cookieJarId, intent, out cookies))
        {
            return controller.BadRequest("Invalid or expired cookie jar ID, or intent mismatch.");
        }

        cookies.WriteToResponse(controller.HttpContext.Response);
        return null;
    }

    public static CookieJarId RegisterAuthSessionCookies(this AppSessionStore sessionStore, IAuthProviderSession authSession, IEnumerable<string>? providersToDelete = null)
    {
        var cookies = new CookieJar();
        cookies.AddCookiesForAuthToken(authSession.AuthToken, providersToDelete);
        cookies.AddCookiesForAuthSessionData(authSession.AuthSessionData);
        cookies.AddCookiesForOAuthProviderSessions(authSession.OAuthProviderSessions);
        return sessionStore.RegisterCookies(cookies, CookieJarIntents.SetAuthCookies);
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

    public static void AddCookiesForAuthToken(this CookieJar cookies, AuthToken? authToken, IEnumerable<string>? providersToDelete = null)
    {
        var authTokenName = "access_token";
        var refreshTokenName = "refresh_token";
        var tagName = "auth_tag";

        if (string.IsNullOrEmpty(authToken?.AccessToken))
        {
            cookies.Delete(authTokenName, CreateAuthCookieOptions());
            cookies.Delete(refreshTokenName, CreateAuthCookieOptions());
            cookies.Delete(tagName, CreateAuthCookieOptions());

            // Delete OAuth provider session cookies
            if (providersToDelete != null)
            {
                var cookieOptions = CreateAuthCookieOptions();
                foreach (var provider in providersToDelete)
                {
                    cookies.Delete($"{provider}_access_token", cookieOptions);
                    cookies.Delete($"{provider}_refresh_token", cookieOptions);
                    cookies.Delete($"{provider}_auth_tag", cookieOptions);
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
                var tagJson = JsonSerializer.Serialize(authToken.Tag, JsonHelper.DefaultOptions);
                cookies.Append(tagName, tagJson, cookieOptions);
            }
            else
            {
                cookies.Delete(tagName, CreateAuthCookieOptions());
            }
        }
    }

    private static void AddCookiesForAuthSessionData(this CookieJar cookies, string? authSessionData)
    {
        var authSessionDataName = "auth_session_data";

        if (authSessionData == null)
        {
            cookies.Delete(authSessionDataName, CreateAuthCookieOptions());
        }
        else
        {
            var cookieOptions = CreateAuthCookieOptions();

            cookies.Append(authSessionDataName, authSessionData, cookieOptions);
        }
    }

    public static void AddCookiesForOAuthProviderSessions(this CookieJar cookies, IReadOnlyDictionary<string, IAuthTokenHandlerSession> oauthProviderSessions)
    {
        var cookieOptions = CreateAuthCookieOptions();

        foreach (var (provider, session) in oauthProviderSessions)
        {
            var accessTokenName = $"{provider}_access_token";
            var refreshTokenName = $"{provider}_refresh_token";
            var tagName = $"{provider}_auth_tag";

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
                var tagJson = JsonSerializer.Serialize(session.AuthToken.Tag, JsonHelper.DefaultOptions);
                cookies.Append(tagName, tagJson, cookieOptions);
            }
            else
            {
                cookies.Delete(tagName, CreateAuthCookieOptions());
            }
        }
    }

    private static CookieOptions CreateAuthCookieOptions()
    {
        var isProduction = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = isProduction,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            Path = "/",
        };

        // Apply custom configuration if provided
        global::Ivy.Server.ConfigureAuthCookieOptions?.Invoke(cookieOptions);

        return cookieOptions;
    }
}
