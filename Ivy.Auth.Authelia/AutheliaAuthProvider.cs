using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Ivy.Hooks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Ivy.Auth.Authelia;

public class AutheliaAuthProvider : IAuthProvider
{
    private readonly HttpClient _httpClient;
    private readonly CookieContainer _cookieContainer;
    private readonly string _baseUrl;

    public AutheliaAuthProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetEntryAssembly()!)
            .Build();
        _baseUrl = configuration.GetValue<string>("AUTHELIA_URL")
            ?? throw new Exception("AUTHELIA_URL is required");
        _cookieContainer = new CookieContainer();
        var handler = new HttpClientHandler { CookieContainer = _cookieContainer };
        _httpClient = new HttpClient(handler) { BaseAddress = new Uri(_baseUrl) };
    }

    public async Task<AuthToken?> LoginAsync(string username, string password)
    {
        var payload = new { username, password };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("/api/firstfactor", content);
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

    public async Task LogoutAsync(string _)
    {
        // Instruct Authelia to log out. Then expire the session cookie.
        await _httpClient.PostAsync("/api/logout", new StringContent(string.Empty));
        var expired = new Cookie("authelia_session", "", "/", new Uri(_baseUrl).Host)
        {
            Expires = DateTime.UtcNow.AddDays(-1)
        };
        _cookieContainer.Add(new Uri(_baseUrl), expired);
    }

    public Task<AuthToken?> RefreshJwtAsync(AuthToken jwt) => Task.FromResult<AuthToken?>(jwt);

    public Task<Uri> GetOAuthUriAsync(AuthOption option, WebhookEndpoint callback)
    {
        throw new NotImplementedException();
    }

    public Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> ValidateJwtAsync(string jwt)
    {
        // Send a request with the session cookie to /api/user/info.
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/user/info");
        request.Headers.Add("Cookie", $"authelia_session={jwt}");
        var response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    public async Task<UserInfo?> GetUserInfoAsync(string jwt)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/user/info");
        request.Headers.Add("Cookie", $"authelia_session={jwt}");
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return null;
        var json = await response.Content.ReadAsStringAsync();
        var user = JsonSerializer.Deserialize<AutheliaUser>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (user == null)
            return null;
        return new UserInfo(user.Id, user.Email, user.DisplayName, null);
    }

    public AuthOption[] GetAuthOptions()
    {
        return [new AuthOption(AuthFlow.EmailPassword)];
    }
}

public class AutheliaUser
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}
