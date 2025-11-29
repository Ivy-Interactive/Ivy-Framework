using System.Net;
using System.Text.Json;
using Ivy.Cookies;
using Microsoft.AspNetCore.Http;

namespace Ivy.Auth;

public static class CookieRegistryExtensions
{
    public static CookieJarId Register(this ICookieRegistry cookieRegistry, AuthToken? authToken)
    {
        var cookies = new CookieJar();
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
        return cookieRegistry.Register(cookies, CookieJarIntents.SetAuthToken);
    }
}
