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
            Console.WriteLine("Deleting JWT cookie because passed-in JWT is null.");
            HttpContext.Response.Cookies.Delete("jwt");
        }
        else
        {
            Console.WriteLine($"Adding JWT cookie: {token}.");
            HttpContext.Response.Cookies.Append("jwt", JsonSerializer.Serialize(token), new CookieOptions
            {
                HttpOnly = true,
                //Secure = true, //todo: enable this in production
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.MaxValue
            });
        }
        return Ok();
    }
}
