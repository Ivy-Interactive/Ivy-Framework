using System.Security.Claims;
using System.Text;
using Ivy.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Supabase;
using Supabase.Gotrue;
using GotrueConstants = global::Supabase.Gotrue.Constants;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace Ivy.Auth.Supabase;

public class SupabaseOAuthException(string? error, string? errorCode, string? errorDescription)
    : Exception($"Supabase error: '{error}', code '{errorCode}' - {errorDescription}")
{
    public string? Error { get; } = error;
    public string? ErrorCode { get; } = errorCode;
    public string? ErrorDescription { get; } = errorDescription;
}

public class SupabaseAuthProvider : SupabaseAuthTokenHandler, IAuthProvider
{
    private readonly List<AuthOption> _authOptions = new();

    private string? _pkceCodeVerifier = null;

    public SupabaseAuthProvider(IConfiguration configuration)
        : base(
            CreateHttpClient(configuration),
            GetIssuer(configuration),
            GetJwksUrl(configuration),
            CreateLegacyJwtKey(configuration),
            CreateClient(configuration))
    {
    }

    private static global::Supabase.Client CreateClient(IConfiguration configuration)
    {
        var url = configuration.GetValue<string>("Supabase:Url") ?? throw new Exception("Supabase:Url is required");
        var apiKey = configuration.GetValue<string>("Supabase:ApiKey") ?? throw new Exception("Supabase:ApiKey is required");

        var options = new SupabaseOptions
        {
            AutoRefreshToken = false,
            AutoConnectRealtime = false
        };

        return new global::Supabase.Client(url, apiKey, options);
    }

    private static HttpClient CreateHttpClient(IConfiguration configuration)
    {
        var userAgent = AuthProviderHelpers.GetUserAgent(configuration, "Supabase:UserAgent");
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
        return httpClient;
    }

    private static string GetIssuer(IConfiguration configuration)
    {
        var url = configuration.GetValue<string>("Supabase:Url") ?? throw new Exception("Supabase:Url is required");
        return new Uri(new Uri(url), "auth/v1").ToString();
    }

    private static string GetJwksUrl(IConfiguration configuration)
    {
        var issuer = GetIssuer(configuration);
        return $"{issuer}/.well-known/jwks.json";
    }

    private static SymmetricSecurityKey? CreateLegacyJwtKey(IConfiguration configuration)
    {
        var legacyJwtSecret = configuration.GetValue<string?>("Supabase:LegacyJwtSecret");
        if (!string.IsNullOrEmpty(legacyJwtSecret))
        {
            var keyBytes = Encoding.UTF8.GetBytes(legacyJwtSecret);
            return new SymmetricSecurityKey(keyBytes);
        }
        return null;
    }

    public async Task<AuthToken?> LoginAsync(IAuthSession authSession, string email, string password, CancellationToken cancellationToken)
    {
        var session = await Client.Auth.SignIn(email, password)
            .WaitAsync(cancellationToken);
        var authToken = MakeAuthToken(session);
        return authToken;
    }

    public async Task<Uri> GetOAuthUriAsync(IAuthSession authSession, AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken)
    {
        var provider = option.Id switch
        {
            "google" => GotrueConstants.Provider.Google,
            "apple" => GotrueConstants.Provider.Apple,
            "discord" => GotrueConstants.Provider.Discord,
            "twitch" => GotrueConstants.Provider.Twitch,
            "figma" => GotrueConstants.Provider.Figma,
            "notion" => GotrueConstants.Provider.Notion,
            "azure" => GotrueConstants.Provider.Azure,
            "workos" => GotrueConstants.Provider.WorkOS,
            "github" => GotrueConstants.Provider.Github,
            "gitlab" => GotrueConstants.Provider.Gitlab,
            "bitbucket" => GotrueConstants.Provider.Bitbucket,
            _ => throw new ArgumentException($"Unknown OAuth provider: {option.Id}"),
        };

        var signInOptions = new SignInOptions
        {
            RedirectTo = callback.GetUri().ToString(),
            FlowType = GotrueConstants.OAuthFlowType.PKCE,
        };

        // Set scopes. These are necessary for Discord, but some providers return errors if they're provided.
        if (provider != GotrueConstants.Provider.Gitlab
            && provider != GotrueConstants.Provider.Figma
            && provider != GotrueConstants.Provider.Twitch
            && provider != GotrueConstants.Provider.WorkOS)
        {
            signInOptions.Scopes = "email openid";
        }

        if (provider == GotrueConstants.Provider.WorkOS)
        {
            if (option.Tag is not string connectionId || string.IsNullOrEmpty(connectionId))
            {
                throw new ArgumentException("WorkOS connection ID not provided.");
            }

            signInOptions.QueryParams = new()
            {
                ["connection"] = connectionId,
            };
        }
        else if (provider == GotrueConstants.Provider.Google)
        {
            // These parameters are needed to get a refresh token from Google.
            signInOptions.QueryParams = new()
            {
                ["access_type"] = "offline",
                ["prompt"] = "consent",
            };
        }

        var providerAuthState = await Client.Auth.SignIn(provider, signInOptions)
            .WaitAsync(cancellationToken);
        _pkceCodeVerifier = providerAuthState.PKCEVerifier;

        return providerAuthState.Uri;
    }

    public async Task<AuthToken?> HandleOAuthCallbackAsync(IAuthSession authSession, HttpRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_pkceCodeVerifier))
        {
            throw new InvalidOperationException("PKCE code verifier is not set. OAuth flow was not properly initiated.");
        }

        var code = request.Query["code"].ToString();

        if (string.IsNullOrWhiteSpace(code))
        {
            var error = request.Query["error"].ToString();
            var errorCode = request.Query["error_code"].ToString();
            var errorDescription = request.Query["error_description"].ToString();

            throw new SupabaseOAuthException(error, errorCode, errorDescription);
        }

        try
        {
            var session = await Client.Auth.ExchangeCodeForSession(_pkceCodeVerifier, code)
                .WaitAsync(cancellationToken);

            ExtractAndStoreProviderTokens(session, authSession);

            return MakeAuthToken(session);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to exchange authorization code: {ex.Message}", ex);
        }
    }

    public async Task LogoutAsync(IAuthSession authSession, CancellationToken cancellationToken)
    {
        await Client.Auth.SignOut()
            .WaitAsync(cancellationToken);
    }

    public AuthOption[] GetAuthOptions()
    {
        return _authOptions.ToArray();
    }

    public SupabaseAuthProvider UseEmailPassword()
    {
        _authOptions.Add(new AuthOption(AuthFlow.EmailPassword));
        return this;
    }

    public SupabaseAuthProvider UseGoogle()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Google", nameof(GotrueConstants.Provider.Google).ToLower(), Icons.Google));
        return this;
    }

    public SupabaseAuthProvider UseApple()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Apple", nameof(GotrueConstants.Provider.Apple).ToLower(), Icons.Apple));
        return this;
    }

    public SupabaseAuthProvider UseDiscord()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Discord", nameof(GotrueConstants.Provider.Discord).ToLower(), Icons.Discord));
        return this;
    }

    public SupabaseAuthProvider UseTwitch()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Twitch", nameof(GotrueConstants.Provider.Twitch).ToLower(), Icons.Twitch));
        return this;
    }

    public SupabaseAuthProvider UseFigma()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Figma", nameof(GotrueConstants.Provider.Figma).ToLower(), Icons.Figma));
        return this;
    }

    public SupabaseAuthProvider UseNotion()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Notion", nameof(GotrueConstants.Provider.Notion).ToLower(), Icons.Notion));
        return this;
    }

    public SupabaseAuthProvider UseAzure()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Azure", nameof(GotrueConstants.Provider.Azure).ToLower(), Icons.Azure));
        return this;
    }

    public SupabaseAuthProvider UseWorkOS(string connectionId)
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "WorkOS", nameof(GotrueConstants.Provider.WorkOS).ToLower(), Icons.None, connectionId));
        return this;
    }

    public SupabaseAuthProvider UseGithub()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "GitHub", nameof(GotrueConstants.Provider.Github).ToLower(), Icons.Github));
        return this;
    }

    public SupabaseAuthProvider UseGitlab()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "GitLab", nameof(GotrueConstants.Provider.Gitlab).ToLower(), Icons.Gitlab));
        return this;
    }

    public SupabaseAuthProvider UseBitbucket()
    {
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Bitbucket", nameof(GotrueConstants.Provider.Bitbucket).ToLower(), Icons.Bitbucket));
        return this;
    }

    private static void ExtractAndStoreProviderTokens(Session? session, IAuthSession authSession)
    {
        if (session?.User?.AppMetadata == null || string.IsNullOrEmpty(session.ProviderToken))
        {
            return;
        }

        if (!session.User.UserMetadata.TryGetValue("provider_id", out var providerIdObj) || providerIdObj is not string providerId)
        {
            return;
        }

        OAuthProvider? oauthProvider = null;

        foreach (var identity in session.User.Identities)
        {
            if (string.IsNullOrEmpty(identity.Id) || identity.Id != providerId)
            {
                continue;
            }

            oauthProvider = identity.Provider?.ToLowerInvariant() switch
            {
                "google" => OAuthProvider.Google,
                "github" => OAuthProvider.GitHub,
                "apple" => OAuthProvider.Apple,
                "microsoft" => OAuthProvider.Microsoft,
                "twitter" => OAuthProvider.Twitter,
                "discord" => OAuthProvider.Discord,
                "twitch" => OAuthProvider.Twitch,
                "figma" => OAuthProvider.Figma,
                "notion" => OAuthProvider.Notion,
                "azure" => OAuthProvider.Azure,
                "workos" => OAuthProvider.WorkOS,
                "gitlab" => OAuthProvider.GitLab,
                "bitbucket" => OAuthProvider.Bitbucket,
                _ => null
            };

            if (oauthProvider.HasValue)
            {
                break;
            }
        }

        if (!oauthProvider.HasValue)
        {
            return;
        }

        var providerAuthToken = new AuthToken(
            session.ProviderToken,
            session.ProviderRefreshToken);

        var providerToken = new OAuthProviderToken(
            Provider: oauthProvider.Value,
            AuthToken: providerAuthToken);

        authSession.AddOAuthProviderToken(providerToken);
    }

    public Task<Dictionary<OAuthProvider, OAuthProviderToken>?> GetOAuthProviderTokensAsync(IAuthSession authSession, CancellationToken cancellationToken = default)
    {
        // Supabase does not provide a way to get the provider tokens outside of the initial authentication flow, so we rely on storing them when we first receive them.
        return Task.FromResult<Dictionary<OAuthProvider, OAuthProviderToken>?>(
            new Dictionary<OAuthProvider, OAuthProviderToken>(authSession.OAuthProviderTokens));
    }

}
