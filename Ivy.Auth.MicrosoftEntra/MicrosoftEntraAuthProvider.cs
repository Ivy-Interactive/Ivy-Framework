using System.Reflection;
using System.Text.Json;
using Ivy.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Ivy.Hooks;
using System.Text.Json.Serialization;

namespace Ivy.Auth.MicrosoftEntra;

public class MicrosoftEntraOAuthException(string? error, string? errorCode, string? errorDescription)
    : Exception($"Microsoft Entra error: '{error}', code '{errorCode}' - {errorDescription}")
{
    public string? Error { get; } = error;
    public string? ErrorCode { get; } = errorCode;
    public string? ErrorDescription { get; } = errorDescription;
}

public class MicrosoftEntraAuthProvider : IAuthProvider
{
    private IConfidentialClientApplication? _app;
    private HttpContext? _httpContext = null;
    private readonly string[] _scopes = ["User.Read", "openid", "profile", "email", "offline_access"];

    private readonly List<AuthOption> _authOptions = [];
    TokenCache? _tokenCache = null;

    private string? _codeVerifier = null;

    private readonly string _tenantId;
    private readonly string _clientId;
    private readonly string _clientSecret;

    record struct TokenCache(Dictionary<string, RefreshToken> RefreshToken);

    record struct RefreshToken([property: JsonPropertyName("secret")] string Secret);

    public MicrosoftEntraAuthProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetEntryAssembly()!)
            .Build();

        _tenantId = configuration.GetValue<string>("MICROSOFT_ENTRA_TENANT_ID") ?? throw new Exception("MICROSOFT_ENTRA_TENANT_ID is required");
        _clientId = configuration.GetValue<string>("MICROSOFT_ENTRA_CLIENT_ID") ?? throw new Exception("MICROSOFT_ENTRA_CLIENT_ID is required");
        _clientSecret = configuration.GetValue<string>("MICROSOFT_ENTRA_CLIENT_SECRET") ?? throw new Exception("MICROSOFT_ENTRA_CLIENT_SECRET is required");
    }

    public void SetHttpContext(HttpContext context) => _httpContext = context;

    private IConfidentialClientApplication GetApp()
    {
        if (_app != null)
        {
            return _app;
        }

        if (_httpContext == null)
        {
            throw new InvalidOperationException("SetHttpContext() must be called before GetApp()");
        }

        // Create a confidential client application for OAuth flow
        var baseUrl = WebhookEndpoint.BuildBaseUrl(_httpContext.Request.Scheme, _httpContext.Request.Host.Value!);

        _app = ConfidentialClientApplicationBuilder
            .Create(_clientId)
            .WithClientSecret(_clientSecret)
            .WithAuthority(new Uri($"https://login.microsoftonline.com/{_tenantId}"))
            .WithRedirectUri(baseUrl)
            .Build();

        _app.UserTokenCache.SetAfterAccess(args =>
        {
            var cacheBytes = args.TokenCache.SerializeMsalV3();
            _tokenCache = JsonSerializer.Deserialize<TokenCache>(cacheBytes);
        });

        return _app;
    }

    public Task<AuthToken?> LoginAsync(string email, string password)
        => throw new InvalidOperationException("Microsoft Entra login with email/password is not supported");

    public async Task<Uri> GetOAuthUriAsync(AuthOption option, WebhookEndpoint callback)
    {
        _codeVerifier = GenerateCodeVerifier();
        var codeChallenge = GenerateCodeChallenge(_codeVerifier);

        var authUrl = await GetApp()
            .GetAuthorizationRequestUrl(_scopes)
            .WithRedirectUri(callback.GetUri(includeIdInPath: false).ToString())
            .WithExtraQueryParameters(new Dictionary<string, string>
            {
                ["code_challenge"] = codeChallenge,
                ["code_challenge_method"] = "S256",
                ["state"] = callback.Id,
            })
            .ExecuteAsync();

        return authUrl;
    }

    public async Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request)
    {
        var code = request.Query["code"].ToString();
        var error = request.Query["error"].ToString();
        var errorDescription = request.Query["error_description"].ToString();

        if (error.Length > 0 || errorDescription.Length > 0)
        {
            throw new MicrosoftEntraOAuthException(error, null, errorDescription);
        }
        else if (code.Length == 0)
        {
            throw new Exception("Received no authorization code from Microsoft Entra.");
        }

        if (_codeVerifier == null)
        {
            throw new Exception("Code verifier not found. OAuth flow was not properly initiated.");
        }

        var result = await GetApp().AcquireTokenByAuthorizationCode(_scopes, code)
            .WithPkceCodeVerifier(_codeVerifier)
            .ExecuteAsync();

        var accountId = result.Account.HomeAccountId!.Identifier;
        return new AuthToken(
            result.AccessToken,
            GetCurrentRefreshToken(accountId),
            result.ExpiresOn,
            accountId
        );
    }

    public Task LogoutAsync(string jwt)
    {
        _tokenCache = null;
        _app = null;

        return Task.CompletedTask;
    }

    public async Task<AuthToken?> RefreshJwtAsync(AuthToken jwt)
    {
        var app = GetApp();

        if (app is not IByRefreshToken refresher
            || jwt.Tag is not JsonElement tag
            || tag.GetString() is not string accountId
            || accountId.Length <= 0)
        {
            return jwt;
        }

        if (jwt.ExpiresAt == null || jwt.RefreshToken == null || DateTimeOffset.UtcNow < jwt.ExpiresAt)
        {
            return jwt;
        }

        try
        {
            var account = await app.GetAccountAsync(accountId);

            if (account != null)
            {
                if (account.HomeAccountId?.Identifier != accountId)
                {
                    throw new Exception("account ID does not match");
                }

                var result = await GetApp().AcquireTokenSilent(_scopes, account)
                    .ExecuteAsync();

                if (result == null)
                {
                    return jwt;
                }

                return new AuthToken(
                    result.AccessToken,
                    GetCurrentRefreshToken(accountId),
                    result.ExpiresOn,
                    accountId
                );
            }
            else
            {
                var result = await refresher.AcquireTokenByRefreshToken(_scopes, jwt.RefreshToken)
                    .ExecuteAsync();

                if (result == null)
                {
                    return jwt;
                }

                if (result.Account.HomeAccountId.Identifier != accountId)
                {
                    throw new Exception("account ID does not match");
                }

                return new AuthToken(
                    result.AccessToken,
                    GetCurrentRefreshToken(accountId),
                    result.ExpiresOn,
                    accountId
                );
            }
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<bool> ValidateJwtAsync(string jwt)
    {
        try
        {
            var user = await GetMeAsync(jwt);
            return user != null;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<UserInfo?> GetUserInfoAsync(string jwt)
    {
        try
        {
            var user = await GetMeAsync(jwt);

            if (user == null || user.Id == null)
            {
                return null;
            }

            return new UserInfo(
                user.Id,
                user.Mail ?? user.UserPrincipalName ?? string.Empty,
                user.DisplayName,
                null
            );
        }
        catch (Exception)
        {
            return null;
        }
    }

    public AuthOption[] GetAuthOptions()
    {
        return [.. _authOptions];
    }

    public MicrosoftEntraAuthProvider UseMicrosoftEntra()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Microsoft", "microsoft", Icons.Microsoft));
        return this;
    }

    private static string GenerateCodeVerifier()
    {
        var bytes = new byte[32];
        System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string GenerateCodeChallenge(string codeVerifier)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(codeVerifier);
        var hash = System.Security.Cryptography.SHA256.HashData(bytes);
        return Convert.ToBase64String(hash)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private string? GetCurrentRefreshToken(string accountId)
    {
        if (_tokenCache is not { } tokenCache)
        {
            return null;
        }

        foreach (var (key, token) in tokenCache.RefreshToken)
        {
            if (key.StartsWith(accountId))
            {
                return token.Secret;
            }
        }

        return null;
    }

    private async Task<Microsoft.Graph.Models.User?> GetMeAsync(string jwt)
    {
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
        var tempGraphClient = new GraphServiceClient(httpClient);
        return await tempGraphClient.Me.GetAsync();
    }
}
