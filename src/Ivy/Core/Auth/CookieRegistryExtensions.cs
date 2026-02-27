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

    public static CookieJarId RegisterAuthSessionCookies(this AppSessionStore sessionStore, IAuthSession authSession)
    {
        var cookies = new CookieJar();
        cookies.AddCookiesForAuthToken(authSession.AuthToken);
        cookies.AddCookiesForAuthSessionData(authSession.AuthSessionData);
        cookies.AddCookiesForOAuthProviderTokens(authSession.OAuthProviderTokens);
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
        var authTokenName = "access_token";
        var refreshTokenName = "refresh_token";
        var tagName = "auth_tag";

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

    private static void AddCookiesForOAuthProviderTokens(this CookieJar cookies, IReadOnlyDictionary<OAuthProvider, OAuthProviderToken> oauthProviderTokens)
    {
        // Clear any existing OAuth provider token cookies that are no longer present
        // We'll handle this by setting cookies for all current providers and deleting old ones on read

        var cookieOptions = CreateAuthCookieOptions();

        foreach (var (provider, token) in oauthProviderTokens)
        {
            var prefix = GetProviderPrefix(provider);
            var accessTokenName = $"{prefix}_access_token";
            var refreshTokenName = $"{prefix}_refresh_token";
            var tagName = $"{prefix}_auth_tag";

            // Store access token
            if (!string.IsNullOrEmpty(token.AuthToken.AccessToken))
            {
                cookies.Append(accessTokenName, token.AuthToken.AccessToken, cookieOptions);
            }
            else
            {
                cookies.Delete(accessTokenName, CreateAuthCookieOptions());
            }

            // Store refresh token if present
            if (!string.IsNullOrEmpty(token.AuthToken.RefreshToken))
            {
                cookies.Append(refreshTokenName, token.AuthToken.RefreshToken, cookieOptions);
            }
            else
            {
                cookies.Delete(refreshTokenName, CreateAuthCookieOptions());
            }

            // Store tag if present
            if (token.AuthToken.Tag != null)
            {
                var tagJson = JsonSerializer.Serialize(token.AuthToken.Tag, JsonHelper.DefaultOptions);
                cookies.Append(tagName, tagJson, cookieOptions);
            }
            else
            {
                cookies.Delete(tagName, CreateAuthCookieOptions());
            }
        }
    }

    private static string GetProviderPrefix(OAuthProvider provider)
    {
        return provider switch
        {
            OAuthProvider.Google => "go",
            OAuthProvider.GitHub => "gh",
            OAuthProvider.Microsoft => "ms",
            OAuthProvider.Apple => "ap",
            OAuthProvider.Twitter => "tw",
            OAuthProvider.Discord => "dc",
            OAuthProvider.Twitch => "tc",
            OAuthProvider.Figma => "fg",
            OAuthProvider.Notion => "nt",
            OAuthProvider.Azure => "az",
            OAuthProvider.WorkOS => "wo",
            OAuthProvider.GitLab => "gl",
            OAuthProvider.Bitbucket => "bb",
            _ => provider.ToString().ToLowerInvariant().Substring(0, Math.Min(2, provider.ToString().Length))
        };
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
