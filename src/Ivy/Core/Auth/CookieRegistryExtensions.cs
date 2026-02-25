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

    public static CookieJarId RegisterAuthSessionCookies(this AppSessionStore sessionStore, IAuthSession authSession, string providerSuffix)
    {
        var cookies = new CookieJar();
        cookies.AddCookiesForAuthToken(authSession.AuthToken, providerSuffix);
        cookies.AddCookiesForAuthSessionData(authSession.AuthSessionData, providerSuffix);
        return sessionStore.RegisterCookies(cookies, CookieJarIntents.SetAuthCookies);
    }

    public static CookieJarId RegisterAuthTokenCookies(this AppSessionStore sessionStore, AuthToken? authToken, string providerSuffix)
    {
        var cookies = new CookieJar();
        cookies.AddCookiesForAuthToken(authToken, providerSuffix);
        return sessionStore.RegisterCookies(cookies, CookieJarIntents.SetAuthCookies);
    }

    public static CookieJarId RegisterAuthSessionDataCookies(this AppSessionStore sessionStore, string? authSessionData, string providerSuffix)
    {
        var cookies = new CookieJar();
        cookies.AddCookiesForAuthSessionData(authSessionData, providerSuffix);
        return sessionStore.RegisterCookies(cookies, CookieJarIntents.SetAuthCookies);
    }

    public static void AddCookiesForAuthToken(this CookieJar cookies, AuthToken? authToken, string providerSuffix)
    {
        var authTokenName = $"auth_token_{providerSuffix}";
        var extRefreshTokenName = $"auth_ext_refresh_token_{providerSuffix}";

        if (string.IsNullOrEmpty(authToken?.AccessToken))
        {
            cookies.Delete(authTokenName, CreateAuthCookieOptions());
            cookies.Delete(extRefreshTokenName, CreateAuthCookieOptions());
        }
        else
        {
            var cookieOptions = CreateAuthCookieOptions();

            var tokenJson = JsonSerializer.Serialize(authToken, JsonHelper.DefaultOptions);

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
                tokenJson = JsonSerializer.Serialize(modifiedToken, JsonHelper.DefaultOptions);
                cookies.Append(extRefreshTokenName, refreshToken, cookieOptions);
            }
            else
            {
                cookies.Delete(extRefreshTokenName, CreateAuthCookieOptions());
            }
            cookies.Append(authTokenName, tokenJson, cookieOptions);
        }
    }

    private static void AddCookiesForAuthSessionData(this CookieJar cookies, string? authSessionData, string providerSuffix)
    {
        var authSessionDataName = $"auth_session_data_{providerSuffix}";

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
