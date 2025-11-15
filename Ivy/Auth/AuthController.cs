using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Auth;

/// <summary>
/// Controller for handling authentication-related HTTP requests.
/// </summary>
public class AuthController() : Controller
{
    /// <summary>
    /// Sets or clears the authentication token in HTTP cookies.
    /// </summary>
    /// <param name="token">The authentication token to set, or null to clear</param>
    /// <returns>OK result indicating the operation completed</returns>
    [Route("ivy/auth/set-auth-token")]
    [HttpPatch]
    public IActionResult SetAuthToken([FromBody] AuthToken? token)
    {
        var cookies = HttpContext.Response.Cookies;
        if (string.IsNullOrEmpty(token?.AccessToken))
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
        }
        return Ok();
    }
}
