using System.Net;
using System.Text.Json;
using Ivy.Core.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Auth;

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

    public static CookieJarId RegisterAuthSessionCookies(this AppSessionStore sessionStore, IAuthSession authSession)
    {
        var cookies = new CookieJar();
        var authToken = authSession.AccessToken != null
            ? new AuthToken(authSession.AccessToken, authSession.RefreshToken)
            : null;
        cookies.AddCookiesForAuthToken(authToken);
        cookies.AddCookiesForAuthSessionData(authSession.AuthSessionData);
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

    public static void AddCookiesForAuthToken(this CookieJar cookies, AuthToken? authToken)
    {
        if (string.IsNullOrEmpty(authToken?.AccessToken))
        {
            cookies.Delete("auth_access_token");
            cookies.Delete("auth_refresh_token");
        }
        else
        {
            var cookieOptions = CreateAuthCookieOptions();
            cookies.Append("auth_access_token", authToken.AccessToken, cookieOptions);

            if (string.IsNullOrEmpty(authToken.RefreshToken))
            {
                cookies.Delete("auth_refresh_token");
            }
            else
            {
                cookies.Append("auth_refresh_token", authToken.RefreshToken, cookieOptions);
            }
        }
    }

    public static void AddCookiesForAuthSessionData(this CookieJar cookies, string? authSessionData)
    {
        if (authSessionData == null)
        {
            cookies.Delete("auth_session_data");
        }
        else
        {
            var cookieOptions = CreateAuthCookieOptions();

            cookies.Append("auth_session_data", authSessionData, cookieOptions);
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
        Server.ConfigureAuthCookieOptions?.Invoke(cookieOptions);

        return cookieOptions;
    }
}
