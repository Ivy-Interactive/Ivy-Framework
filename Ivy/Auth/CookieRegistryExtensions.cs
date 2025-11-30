using System.Net;
using System.Text.Json;
using Ivy.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Auth;

public static class CookieRegistryExtensions
{
    public static IActionResult? WriteCookiesToResponse(this Controller controller, IGlobalCookieRegistry globalCookieRegistry, CookieJarId cookieJarId, string intent, out CookieJar cookies)
    {
        if (!globalCookieRegistry.TryRemove(cookieJarId, intent, out cookies))
        {
            return controller.BadRequest("Invalid or expired cookie jar ID, or intent mismatch.");
        }

        cookies.WriteToResponse(controller.HttpContext.Response);
        return null;
    }

    public static CookieJarId Register(this ICookieRegistry cookieRegistry, IAuthSession authSession)
    {
        var cookies = new CookieJar();
        cookies.AddCookiesForAuthToken(authSession.AuthToken);
        cookies.AddCookiesForAuthSessionData(authSession.AuthSessionData);
        return cookieRegistry.Register(cookies, CookieJarIntents.SetAuthCookies);
    }

    public static CookieJarId Register(this ICookieRegistry cookieRegistry, AuthToken? authToken)
    {
        var cookies = new CookieJar();
        cookies.AddCookiesForAuthToken(authToken);
        return cookieRegistry.Register(cookies, CookieJarIntents.SetAuthCookies);
    }

    public static CookieJarId RegisterAuthSessionData(this ICookieRegistry cookieRegistry, string? authSessionData)
    {
        var cookies = new CookieJar();
        cookies.AddCookiesForAuthSessionData(authSessionData);
        return cookieRegistry.Register(cookies, CookieJarIntents.SetAuthCookies);
    }

    private static void AddCookiesForAuthToken(this CookieJar cookies, AuthToken? authToken)
    {
        if (string.IsNullOrEmpty(authToken?.AccessToken))
        {
            cookies.Delete("auth_token");
            cookies.Delete("auth_ext_refresh_token");
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

            var tokenJson = JsonSerializer.Serialize(authToken);

            // Calculate url-encoded token length
            var tokenJsonLength = WebUtility.UrlEncode(tokenJson).Length;
            var refreshTokenLength = authToken.RefreshToken != null
                ? WebUtility.UrlEncode(authToken.RefreshToken).Length
                : 0;

            // If the token is too big, try putting the refresh token into its own cookie.
            // I'm not trying to be overly precise here.
            const int CookieSizeLimit = 4000;

            if (tokenJsonLength > CookieSizeLimit && tokenJsonLength - refreshTokenLength < CookieSizeLimit)
            {
                var refreshToken = authToken.RefreshToken!; // non-nullness implied by condition above
                var modifiedToken = authToken with { RefreshToken = null };
                tokenJson = JsonSerializer.Serialize(modifiedToken);
                cookies.Append("auth_ext_refresh_token", refreshToken, cookieOptions);
            }
            else
            {
                cookies.Delete("auth_ext_refresh_token");
            }
            cookies.Append("auth_token", tokenJson, cookieOptions);
        }
    }

    private static void AddCookiesForAuthSessionData(this CookieJar cookies, string? authSessionData)
    {
        if (authSessionData == null)
        {
            cookies.Delete("auth_session_data");
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

            if (authSessionData is string sessionDataString)
            {
                cookies.Append("auth_session_data", sessionDataString, cookieOptions);
            }
            else
            {
                var sessionDataJson = JsonSerializer.Serialize(authSessionData);
                cookies.Append("auth_session_data", sessionDataJson, cookieOptions);
            }
        }
    }
}
