using Ivy.Hooks;
using Microsoft.AspNetCore.Http;

namespace Ivy.Auth;

/// <summary>
/// Defines the contract for authentication providers in Ivy applications.
/// </summary>
public interface IAuthProvider
{
    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    /// <param name="email">The user's email address</param>
    /// <param name="password">The user's password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An authentication token if successful, null otherwise</returns>
    Task<AuthToken?> LoginAsync(string email, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs out a user by invalidating their access token.
    /// </summary>
    /// <param name="token">The access token to invalidate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task LogoutAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes an expired or expiring access token.
    /// </summary>
    /// <param name="token">The current authentication token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A new authentication token if successful, null otherwise</returns>
    Task<AuthToken?> RefreshAccessTokenAsync(AuthToken token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether an access token is valid.
    /// </summary>
    /// <param name="token">The access token to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the token is valid, false otherwise</returns>
    Task<bool> ValidateAccessTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves user information using a valid access token.
    /// </summary>
    /// <param name="token">The access token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User information if successful, null otherwise</returns>
    Task<UserInfo?> GetUserInfoAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the available authentication options for this provider.
    /// </summary>
    /// <returns>Array of supported authentication options</returns>
    AuthOption[] GetAuthOptions();

    /// <summary>
    /// Generates an OAuth authorization URI for the specified option.
    /// </summary>
    /// <param name="option">The OAuth authentication option</param>
    /// <param name="callback">The webhook endpoint for handling the OAuth callback</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The OAuth authorization URI</returns>
    Task<Uri> GetOAuthUriAsync(AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken = default);

    /// <summary>
    /// Handles the OAuth callback request and extracts the authentication token.
    /// </summary>
    /// <param name="request">The HTTP request containing OAuth callback data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An authentication token if successful, null otherwise</returns>
    Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the expiration time of the given authentication token.
    /// </summary>
    /// <param name="token">The authentication token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The expiration time if available, null otherwise</returns>
    Task<DateTimeOffset?> GetTokenExpiration(AuthToken token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the HTTP context for the auth provider.
    /// </summary>
    /// <param name="context">The HTTP context</param>
    void SetHttpContext(HttpContext context)
    {
    }
}