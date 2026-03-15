using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ivy.Auth;
using Ivy.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ivy.Auth.Sliplane;

/// <summary>Sliplane OAuth exception</summary>
public class SliplaneOAuthException(string? error, string? errorDescription)
    : Exception($"Sliplane OAuth error: '{error}' - {errorDescription}")
{
    public string? Error { get; } = error;
    public string? ErrorDescription { get; } = errorDescription;
}

/// <summary>
/// Sliplane OAuth2 authentication provider for Ivy applications.
/// Implements the OAuth2 authorization code flow using the Sliplane API.
/// </summary>
public class SliplaneAuthProvider : SliplaneAuthTokenHandler, IAuthProvider
{
    /// <summary>Initialize Sliplane auth provider from configuration</summary>
    public SliplaneAuthProvider(IConfiguration configuration, ILogger<SliplaneAuthTokenHandler>? logger = null)
        : base(configuration, logger)
    {
    }

    /// <summary>Not supported — Sliplane only supports OAuth flow</summary>
    public Task<AuthToken?> LoginAsync(IAuthSession authSession, string email, string password, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Sliplane authentication only supports OAuth flow. Use GetOAuthUriAsync and HandleOAuthCallbackAsync instead.");
    }

    /// <summary>No-op logout — Sliplane tokens are stateless</summary>
    public Task LogoutAsync(IAuthSession authSession, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }



    /// <summary>Returns the Sliplane OAuth auth option</summary>
    public AuthOption[] GetAuthOptions() =>
        [new AuthOption(AuthFlow.OAuth, "Sliplane", "sliplane", Icons.Rocket)];

    /// <summary>Build the Sliplane OAuth authorization URI</summary>
    public Task<Uri> GetOAuthUriAsync(IAuthSession authSession, AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken = default)
    {
        if (option.Id != "sliplane")
        {
            throw new ArgumentException($"Unknown auth option: {option.Id}", nameof(option));
        }

        var callbackUri = callback.GetUri(includeIdInPath: false);

        var authUrl = new UriBuilder(AuthorizationUrl)
        {
            Query = string.Join("&", new[]
            {
                $"client_id={Uri.EscapeDataString(ClientId)}",
                $"redirect_uri={Uri.EscapeDataString(callbackUri.ToString())}",
                $"response_type=code",
                $"scope={Uri.EscapeDataString(Scope)}",
                $"state={Uri.EscapeDataString(callback.Id)}",
            })
        };

        return Task.FromResult(authUrl.Uri);
    }

    /// <summary>Handle the OAuth callback and exchange the authorization code for tokens</summary>
    public async Task<AuthToken?> HandleOAuthCallbackAsync(IAuthSession authSession, HttpRequest request, CancellationToken cancellationToken = default)
    {
        var code = request.Query["code"].ToString();
        var error = request.Query["error"].ToString();
        var errorDescription = request.Query["error_description"].ToString();

        if (error.Length > 0 || errorDescription.Length > 0)
        {
            throw new SliplaneOAuthException(error, errorDescription);
        }

        if (code.Length == 0)
        {
            throw new Exception("Received no authorization code from Sliplane.");
        }

        try
        {
            var scheme = request.Headers.TryGetValue("X-Forwarded-Proto", out var forwardedProto)
                ? forwardedProto.ToString()
                : request.Scheme;

            var redirectUri = $"{scheme}://{request.Host}{request.Path}";

            using var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("client_id", ClientId),
                new KeyValuePair<string, string>("client_secret", ClientSecret),
                new KeyValuePair<string, string>("redirect_uri", redirectUri),
            });

            var response = await HttpClient.PostAsync(TokenUrl, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException(
                    $"Sliplane token exchange failed with status {(int)response.StatusCode}: {errorContent}",
                    null,
                    response.StatusCode
                );
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var tokenResponse = JsonSerializer.Deserialize<SliplaneTokenResponse>(json);

            if (tokenResponse == null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            {
                throw new Exception("Invalid token response from Sliplane.");
            }

            return new AuthToken(
                tokenResponse.AccessToken,
                tokenResponse.RefreshToken
            );
        }
        catch (Exception ex) when (ex is not SliplaneOAuthException and not HttpRequestException)
        {
            throw new InvalidOperationException($"Unexpected error during Sliplane OAuth token exchange: {ex.Message}", ex);
        }
    }
}
