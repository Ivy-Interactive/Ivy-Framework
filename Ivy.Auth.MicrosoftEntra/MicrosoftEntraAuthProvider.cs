using System.Reflection;
using Ivy.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Azure.Identity;

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
    private readonly IConfidentialClientApplication _app;
    private readonly GraphServiceClient _graphServiceClient;
    private readonly string[] _scopes = ["User.Read", "openid", "profile", "email"];

    private readonly List<AuthOption> _authOptions = new();

    private string? _codeVerifier = null;

    public MicrosoftEntraAuthProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetEntryAssembly()!)
            .Build();

        var tenantId = configuration.GetValue<string>("MICROSOFT_ENTRA_TENANT_ID") ?? throw new Exception("MICROSOFT_ENTRA_TENANT_ID is required");
        var clientId = configuration.GetValue<string>("MICROSOFT_ENTRA_CLIENT_ID") ?? throw new Exception("MICROSOFT_ENTRA_CLIENT_ID is required");
        var clientSecret = configuration.GetValue<string>("MICROSOFT_ENTRA_CLIENT_SECRET") ?? throw new Exception("MICROSOFT_ENTRA_CLIENT_SECRET is required");

        _app = ConfidentialClientApplicationBuilder
            .Create(clientId)
            .WithClientSecret(clientSecret)
            .WithAuthority(new Uri($"https://login.microsoftonline.com/{tenantId}"))
            .Build();

        var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
        _graphServiceClient = new GraphServiceClient(credential);
    }

    public async Task<AuthToken?> LoginAsync(string email, string password)
    {
        throw new NotSupportedException("Microsoft Entra does not support password authentication directly. Use OAuth flow instead.");
    }

    public async Task<Uri> GetOAuthUriAsync(AuthOption option, Uri callbackUri)
    {
        _codeVerifier = GenerateCodeVerifier();
        var codeChallenge = GenerateCodeChallenge(_codeVerifier);

        var authUrl = await _app
            .GetAuthorizationRequestUrl(_scopes)
            .WithRedirectUri(callbackUri.ToString())
            .WithExtraQueryParameters(new Dictionary<string, string>
            {
                ["code_challenge"] = codeChallenge,
                ["code_challenge_method"] = "S256"
            })
            .ExecuteAsync();

        return authUrl;
    }

    public async Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request)
    {
        var code = request.Query["code"];
        var error = request.Query["error"];
        var errorDescription = request.Query["error_description"];
        
        if (error.Count > 0 || errorDescription.Count > 0)
        {
            throw new MicrosoftEntraOAuthException(error, null, errorDescription);
        }
        else if (code.Count == 0)
        {
            throw new Exception("Received no authorization code from Microsoft Entra.");
        }

        if (_codeVerifier == null)
        {
            throw new Exception("Code verifier not found. OAuth flow was not properly initiated.");
        }

        try
        {
            var result = await _app.AcquireTokenByAuthorizationCode(_scopes, code)
                .WithPkceCodeVerifier(_codeVerifier)
                .ExecuteAsync();

            var authToken = new AuthToken(
                result.AccessToken,
                result.Account?.HomeAccountId?.Identifier,
                result.ExpiresOn
            );

            return authToken;
        }
        catch (MsalException ex)
        {
            throw new MicrosoftEntraOAuthException(ex.ErrorCode, ex.ErrorCode, ex.Message);
        }
    }

    public async Task LogoutAsync(string jwt)
    {
        // For Microsoft Entra, logout typically involves clearing the token cache
        // The actual logout URL would be handled by the client application
        var accounts = await _app.GetAccountsAsync();
        foreach (var account in accounts)
        {
            await _app.RemoveAsync(account);
        }
    }

    public async Task<AuthToken?> RefreshJwtAsync(AuthToken jwt)
    {
        if (jwt.ExpiresAt == null || jwt.RefreshToken == null || DateTimeOffset.UtcNow < jwt.ExpiresAt)
        {
            // Refresh not needed (or not possible).
            return jwt;
        }

        try
        {
            var accounts = await _app.GetAccountsAsync();
            var account = accounts.FirstOrDefault();
            
            if (account == null)
            {
                return null;
            }

            var result = await _app.AcquireTokenSilent(_scopes, account)
                .ExecuteAsync();

            return new AuthToken(
                result.AccessToken,
                result.Account?.HomeAccountId?.Identifier,
                result.ExpiresOn
            );
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
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
            
            var tempGraphClient = new GraphServiceClient(httpClient);
            var user = await tempGraphClient.Me.GetAsync();
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
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
            
            var tempGraphClient = new GraphServiceClient(httpClient);
            var user = await tempGraphClient.Me.GetAsync();

            if (user == null || user.Id == null)
            {
                return null;
            }

            return new UserInfo(
                user.Id,
                user.Mail ?? user.UserPrincipalName ?? string.Empty,
                user.DisplayName,
                null // Microsoft Graph doesn't provide avatar URL in basic user info
            );
        }
        catch (Exception)
        {
            return null;
        }
    }

    public AuthOption[] GetAuthOptions()
    {
        return _authOptions.ToArray();
    }

    public MicrosoftEntraAuthProvider UseMicrosoftEntra()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Microsoft", "microsoft", Icons.None));
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

}