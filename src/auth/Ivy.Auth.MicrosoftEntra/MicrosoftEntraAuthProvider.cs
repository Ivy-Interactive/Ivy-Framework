using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Ivy.Core;
using System.Text.Json.Serialization;
using System.Security.Claims;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Ivy.Auth.MicrosoftEntra;

public class MicrosoftEntraOAuthException(string? error, string? errorCode, string? errorDescription)
    : Exception($"Microsoft Entra error: '{error}', code '{errorCode}' - {errorDescription}")
{
    public string? Error { get; } = error;
    public string? ErrorCode { get; } = errorCode;
    public string? ErrorDescription { get; } = errorDescription;
}

public class MicrosoftEntraAuthProvider : MicrosoftEntraAuthTokenHandler, IAuthProvider
{
    private string? _codeVerifier = null;

    public MicrosoftEntraAuthProvider(IConfiguration configuration)
        : base(
            GetTenantId(configuration),
            GetClientId(configuration),
            GetClientSecret(configuration),
            ["User.Read", "openid", "profile", "email", "offline_access"],
            CreateConfigurationManager(configuration))
    {
    }

    private static string GetTenantId(IConfiguration configuration)
        => configuration.GetValue<string>("MicrosoftEntra:TenantId") ?? throw new Exception("MicrosoftEntra:TenantId is required");

    private static string GetClientId(IConfiguration configuration)
        => configuration.GetValue<string>("MicrosoftEntra:ClientId") ?? throw new Exception("MicrosoftEntra:ClientId is required");

    private static string GetClientSecret(IConfiguration configuration)
        => configuration.GetValue<string>("MicrosoftEntra:ClientSecret") ?? throw new Exception("MicrosoftEntra:ClientSecret is required");

    private static ConfigurationManager<OpenIdConnectConfiguration> CreateConfigurationManager(IConfiguration configuration)
    {
        var tenantId = GetTenantId(configuration);
        return new ConfigurationManager<OpenIdConnectConfiguration>(
            $"https://login.microsoftonline.com/{tenantId}/.well-known/openid-configuration",
            new OpenIdConnectConfigurationRetriever()
        );
    }

    public Task InitializeAsync(IAuthProviderSession authSession, string requestScheme, string requestHost, CancellationToken cancellationToken = default)
    {
        var baseUrl = WebhookEndpoint.BuildAuthCallbackBaseUrl(requestScheme, requestHost);
        SetBaseUrl(baseUrl);
        return Task.CompletedTask;
    }

    public Task<AuthToken?> LoginAsync(IAuthProviderSession authSession, string email, string password, CancellationToken cancellationToken)
        => throw new InvalidOperationException("Microsoft Entra login with email/password is not supported");

    public async Task<Uri> GetOAuthUriAsync(IAuthProviderSession authSession, AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken)
    {
        _codeVerifier = GenerateCodeVerifier();
        var codeChallenge = GenerateCodeChallenge(_codeVerifier);

        var authUrl = await GetApp()
            .GetAuthorizationRequestUrl(Scopes)
            .WithRedirectUri(callback.GetUri(includeIdInPath: false).ToString())
            .WithExtraQueryParameters(new Dictionary<string, (string, bool)>
            {
                ["code_challenge"] = (codeChallenge, false),
                ["code_challenge_method"] = ("S256", false),
                ["state"] = (callback.Id, false),
            })
            .ExecuteAsync(cancellationToken);

        return authUrl;
    }

    public async Task<AuthToken?> HandleOAuthCallbackAsync(IAuthProviderSession authSession, HttpRequest request, CancellationToken cancellationToken)
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

        var result = await GetApp().AcquireTokenByAuthorizationCode(Scopes, code)
            .WithPkceCodeVerifier(_codeVerifier)
            .ExecuteAsync(cancellationToken);

        var accountId = result.Account.HomeAccountId!.Identifier;
        return new AuthToken(
            result.IdToken,
            GetCurrentRefreshToken(accountId),
            accountId
        );
    }

    public Task LogoutAsync(IAuthProviderSession authSession, CancellationToken cancellationToken)
    {
        ClearApp();
        return Task.CompletedTask;
    }

    public AuthOption[] GetAuthOptions() => [new AuthOption(AuthFlow.OAuth, "Microsoft", "microsoft", Icons.Microsoft)];

    [Obsolete("Microsoft Entra OAuth is now enabled by default. This method is no longer necessary and will be removed in a future version.")]
    public MicrosoftEntraAuthProvider UseMicrosoftEntra() => this;

    public async Task<Dictionary<OAuthProvider, OAuthProviderToken>?> GetOAuthProviderTokensAsync(IAuthProviderSession authSession, CancellationToken cancellationToken = default)
    {
        // Return stored sessions converted to tokens if available
        if (authSession.OAuthProviderSessions.Count > 0)
        {
            return authSession.OAuthProviderSessions.ToDictionary(
                kvp => kvp.Key,
                kvp => new OAuthProviderToken(kvp.Key, kvp.Value.AuthToken ?? new AuthToken("", null)));
        }

        if (authSession.AuthToken is not { } token)
        {
            return null;
        }

        try
        {
            var app = GetApp();

            if (app is not IByRefreshToken refresher
                || token.Tag is not JsonElement tag
                || tag.GetString() is not string accountId
                || accountId.Length <= 0
                || token.RefreshToken == null)
            {
                return null;
            }

            // Use refresh token to get a fresh access token for Microsoft Graph
            var result = await refresher.AcquireTokenByRefreshToken(Scopes, token.RefreshToken)
                .ExecuteAsync(cancellationToken);

            if (result?.AccessToken == null)
            {
                return null;
            }

            var tokens = new Dictionary<OAuthProvider, OAuthProviderToken>
            {
                [OAuthProvider.Microsoft] = new OAuthProviderToken(
                    Provider: OAuthProvider.Microsoft,
                    AuthToken: new AuthToken(result.AccessToken),
                    Scopes: result.Scopes?.ToArray(),
                    ExpiresAt: result.ExpiresOn)
            };

            return tokens;
        }
        catch (Exception)
        {
            return null;
        }
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
