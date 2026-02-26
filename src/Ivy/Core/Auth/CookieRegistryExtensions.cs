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

    public static CookieJarId RegisterAuthSessionCookies(this AppSessionStore sessionStore, IAuthSession authSession, string providerPrefix)
    {
        var cookies = new CookieJar();
        cookies.AddCookiesForAuthToken(authSession.AuthToken, providerPrefix);
        cookies.AddCookiesForAuthSessionData(authSession.AuthSessionData, providerPrefix);
        return sessionStore.RegisterCookies(cookies, CookieJarIntents.SetAuthCookies);
    }

    public static CookieJarId RegisterAuthTokenCookies(this AppSessionStore sessionStore, AuthToken? authToken, string providerPrefix)
    {
        var cookies = new CookieJar();
        cookies.AddCookiesForAuthToken(authToken, providerPrefix);
        return sessionStore.RegisterCookies(cookies, CookieJarIntents.SetAuthCookies);
    }

    public static CookieJarId RegisterAuthSessionDataCookies(this AppSessionStore sessionStore, string? authSessionData, string providerPrefix)
    {
        var cookies = new CookieJar();
        cookies.AddCookiesForAuthSessionData(authSessionData, providerPrefix);
        return sessionStore.RegisterCookies(cookies, CookieJarIntents.SetAuthCookies);
    }

    public static void AddCookiesForAuthToken(this CookieJar cookies, AuthToken? authToken, string providerPrefix)
    {
        var authTokenName = $"{providerPrefix}_access_token";
        var refreshTokenName = $"{providerPrefix}_refresh_token";
        var tagName = $"{providerPrefix}_auth_tag";

        if (string.IsNullOrEmpty(authToken?.AccessToken))
        {
            cookies.Delete(authTokenName, CreateAuthCookieOptions());
            cookies.Delete(refreshTokenName, CreateAuthCookieOptions());
            cookies.Delete(tagName, CreateAuthCookieOptions());
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

    private static void AddCookiesForAuthSessionData(this CookieJar cookies, string? authSessionData, string providerPrefix)
    {
        var authSessionDataName = $"{providerPrefix}_auth_session_data";

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
