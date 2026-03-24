using System.Net;
using System.Text;
using System.Text.Json;
using Ivy.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Ivy.Auth.Authelia;

public class AutheliaAuthProvider : AutheliaAuthTokenHandler, IAuthProvider
{
    public AutheliaAuthProvider(IConfiguration configuration)
        : base(configuration)
    {
    }

    public async Task<AuthToken?> LoginAsync(IAuthSession authSession, string username, string password, CancellationToken cancellationToken)
    {
        var payload = new { username, password };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await HttpClient.PostAsync("/api/firstfactor", content, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            // Return the "authelia_session" cookie value as our token.
            var cookies = CookieContainer.GetCookies(new Uri(BaseUrl));
            var session = cookies["authelia_session"]?.Value;
            return session != null
                ? new AuthToken(session)
                : null;
        }
        return null;
    }

    public async Task LogoutAsync(IAuthSession authSession, CancellationToken cancellationToken)
    {
        // Instruct Authelia to log out. Then expire the session cookie.
        await HttpClient.PostAsync("/api/logout", new StringContent(string.Empty), cancellationToken);
        var expired = new Cookie("authelia_session", "", "/", new Uri(BaseUrl).Host)
        {
            Expires = DateTime.UtcNow.AddDays(-1)
        };
        CookieContainer.Add(new Uri(BaseUrl), expired);
    }

    public Task<Uri> GetOAuthUriAsync(IAuthSession authSession, AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<AuthToken?> HandleOAuthCallbackAsync(IAuthSession authSession, HttpRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public AuthOption[] GetAuthOptions()
    {
        return [new AuthOption(AuthFlow.EmailPassword)];
    }
}
