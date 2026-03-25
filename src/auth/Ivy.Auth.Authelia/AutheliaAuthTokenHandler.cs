using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Ivy.Auth.Authelia;

public class AutheliaAuthTokenHandler : IAuthTokenHandler
{
    protected HttpClient HttpClient;
    protected readonly CookieContainer CookieContainer;
    protected readonly string BaseUrl;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    public AutheliaAuthTokenHandler(IConfiguration configuration)
    {
        BaseUrl = configuration.GetValue<string>("Authelia:Url")
            ?? throw new Exception("Authelia:Url is required");
        var userAgent = AuthProviderHelpers.GetUserAgent(configuration, "Authelia:UserAgent");

        CookieContainer = new CookieContainer();
        var handler = new HttpClientHandler { CookieContainer = CookieContainer };
        HttpClient = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
        HttpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
    }

    public async Task<AuthToken?> RefreshAccessTokenAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken)
    {
        // Authelia session tokens cannot be refreshed - validate and return null if invalid
        var isValid = await ValidateAccessTokenAsync(authSession, cancellationToken);
        return isValid ? authSession.AuthToken : null;
    }

    public async Task<bool> ValidateAccessTokenAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken)
    {
        // Send a request with the session cookie to /api/user/info.
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/user/info");
        request.Headers.Add("Cookie", $"authelia_session={authSession.AuthToken?.AccessToken}");
        var response = await HttpClient.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<UserInfo?> GetUserInfoAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/user/info");
        request.Headers.Add("Cookie", $"authelia_session={authSession.AuthToken?.AccessToken}");
        var response = await HttpClient.SendAsync(request, cancellationToken);
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

    public Task<TokenLifetime?> GetAccessTokenLifetimeAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken)
    {
        return Task.FromResult<TokenLifetime?>(null);
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
