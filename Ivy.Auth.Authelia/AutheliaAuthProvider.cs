using System.Linq;
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

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    public AutheliaAuthProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetEntryAssembly()!)
            .Build();
        _baseUrl = configuration.GetValue<string>("Authelia:Url")
            ?? throw new Exception("Authelia:Url is required");
        _cookieContainer = new CookieContainer();
        var handler = new HttpClientHandler { CookieContainer = _cookieContainer };
        _httpClient = new HttpClient(handler) { BaseAddress = new Uri(_baseUrl) };
    }

    public async Task<AuthToken?> LoginAsync(string username, string password, CancellationToken cancellationToken)
    {
        var payload = new { username, password };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("/api/firstfactor", content, cancellationToken);
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

    public async Task LogoutAsync(string _, CancellationToken cancellationToken)
    {
        // Instruct Authelia to log out. Then expire the session cookie.
        await _httpClient.PostAsync("/api/logout", new StringContent(string.Empty), cancellationToken);
        var expired = new Cookie("authelia_session", "", "/", new Uri(_baseUrl).Host)
        {
            Expires = DateTime.UtcNow.AddDays(-1)
        };
        _cookieContainer.Add(new Uri(_baseUrl), expired);
    }

    public async Task<AuthToken?> RefreshAccessTokenAsync(AuthToken token, CancellationToken cancellationToken)
    {
        // Authelia session tokens cannot be refreshed - validate and return null if invalid
        var isValid = await ValidateAccessTokenAsync(token.AccessToken, cancellationToken);
        return isValid ? token : null;
    }

    public Task<Uri> GetOAuthUriAsync(AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> ValidateAccessTokenAsync(string token, CancellationToken cancellationToken)
    {
        // Send a request with the session cookie to /api/user/info.
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/user/info");
        request.Headers.Add("Cookie", $"authelia_session={token}");
        var response = await _httpClient.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<UserInfo?> GetUserInfoAsync(string token, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/user/info");
        request.Headers.Add("Cookie", $"authelia_session={token}");
        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var wrapper = JsonSerializer.Deserialize<AutheliaUserInfoResponse>(json, _jsonOptions);
        if (wrapper?.Data == null)
            return null;
        var displayName = wrapper.Data.DisplayName ?? string.Empty;
        var email = wrapper.Data.Emails?.FirstOrDefault();
        return email != null
            ? new UserInfo(displayName, email, displayName, null)
            : null;
    }

    public AuthOption[] GetAuthOptions()
    {
        return [new AuthOption(AuthFlow.EmailPassword)];
    }

    public Task<DateTimeOffset?> GetTokenExpiration(AuthToken token, CancellationToken cancellationToken)
    {
        return Task.FromResult<DateTimeOffset?>(null);
    }
}

public class AutheliaUserInfoResponse
{
    public string? Status { get; set; }
    public AutheliaUserInfoData? Data { get; set; }
}

public class AutheliaUserInfoData
{
    public string? DisplayName { get; set; }
    public string? Method { get; set; }
    public bool HasWebauthn { get; set; }
    public bool HasTotp { get; set; }
    public bool HasDuo { get; set; }
    public string[]? Emails { get; set; }
}
