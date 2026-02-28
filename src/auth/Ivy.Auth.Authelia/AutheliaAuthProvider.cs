using System.Net;
using System.Text;
using System.Text.Json;
using Ivy.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Ivy.Auth.Authelia;

public class AutheliaAuthProvider : AutheliaAuthTokenHandler, IAuthProvider
{
    private readonly CookieContainer _cookieContainer;
    private readonly string _baseUrl;

    public AutheliaAuthProvider(IConfiguration configuration)
        : base()
    {
        HttpClient = CreateHttpClient(configuration, out var cookieContainer, out var baseUrl);
        _cookieContainer = cookieContainer;
        _baseUrl = baseUrl;
    }

    private static HttpClient CreateHttpClient(IConfiguration configuration, out CookieContainer cookieContainer, out string baseUrl)
    {
        baseUrl = configuration.GetValue<string>("Authelia:Url")
            ?? throw new Exception("Authelia:Url is required");
        var userAgent = AuthProviderHelpers.GetUserAgent(configuration, "Authelia:UserAgent");

        cookieContainer = new CookieContainer();
        var handler = new HttpClientHandler { CookieContainer = cookieContainer };
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };
        httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
        return httpClient;
    }

    public async Task<AuthToken?> LoginAsync(IAuthProviderSession authSession, string username, string password, CancellationToken cancellationToken)
    {
        var payload = new { username, password };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await HttpClient.PostAsync("/api/firstfactor", content, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            // Return the "authelia_session" cookie value as our token.
            var cookies = _cookieContainer.GetCookies(new Uri(_baseUrl));
            var session = cookies["authelia_session"]?.Value;
            return session != null
                ? new AuthToken(session)
                : null;
        }
        return null;
    }

    public async Task LogoutAsync(IAuthProviderSession authSession, CancellationToken cancellationToken)
    {
        // Instruct Authelia to log out. Then expire the session cookie.
        await HttpClient.PostAsync("/api/logout", new StringContent(string.Empty), cancellationToken);
        var expired = new Cookie("authelia_session", "", "/", new Uri(_baseUrl).Host)
        {
            Expires = DateTime.UtcNow.AddDays(-1)
        };
        _cookieContainer.Add(new Uri(_baseUrl), expired);
    }

    public Task<Uri> GetOAuthUriAsync(IAuthProviderSession authSession, AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<AuthToken?> HandleOAuthCallbackAsync(IAuthProviderSession authSession, HttpRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public AuthOption[] GetAuthOptions()
    {
        return [new AuthOption(AuthFlow.EmailPassword)];
    }
}
