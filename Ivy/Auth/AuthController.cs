using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Auth;

public class AuthController() : Controller
{
    [Route("auth/set-jwt")]
    [HttpPatch]
    public IActionResult SetJwt([FromBody] AuthToken? token)
    {
        var cookies = HttpContext.Response.Cookies;
        if (string.IsNullOrEmpty(token?.Jwt))
        {
            cookies.Delete("jwt");
            cookies.Delete("jwt_ext_refresh_token");
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

            // If the token is too big, try putting the refresh token into its own cookie.
            // I'm not trying to be overly precise here.
            const int CookieSizeLimit = 4000;
            if (tokenJson.Length > CookieSizeLimit && tokenJson.Length - (token.RefreshToken?.Length ?? 0) < CookieSizeLimit)
            {
                var refreshToken = token.RefreshToken!; // non-nullness implied by condition above
                var modifiedToken = token with { RefreshToken = null };
                tokenJson = JsonSerializer.Serialize(modifiedToken);
                cookies.Append("jwt_ext_refresh_token", refreshToken, cookieOptions);
            }
            else
            {
                cookies.Delete("jwt_ext_refresh_token");
            }
            cookies.Append("jwt", tokenJson, cookieOptions);
        }
        return Ok();
    }
}
