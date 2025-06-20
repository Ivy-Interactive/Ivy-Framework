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
        if (string.IsNullOrEmpty(token?.Jwt))
        {
            HttpContext.Response.Cookies.Delete("jwt");
        }
        else
        {
            var isProduction = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";
            HttpContext.Response.Cookies.Append("jwt", JsonSerializer.Serialize(token), new CookieOptions
            {
                HttpOnly = true,
                Secure = isProduction, // Enable Secure flag in production
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddYears(1),
            });
        }
        return Ok();
    }
}
