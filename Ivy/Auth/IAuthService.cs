using Ivy.Hooks;
using Microsoft.AspNetCore.Http;

namespace Ivy.Auth;

/// <summary>
/// Service interface for authentication operations in Ivy applications.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    /// <param name="email">The user's email address</param>
    /// <param name="password">The user's password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An authentication token if successful, null otherwise</returns>
    Task<AuthToken?> LoginAsync(string email, string password, CancellationToken cancellationToken);

    /// <summary>
    /// Generates an OAuth authorization URI for the specified option.
    /// </summary>
    /// <param name="option">The OAuth authentication option</param>
    /// <param name="callback">The webhook endpoint for handling the OAuth callback</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The OAuth authorization URI</returns>
    Task<Uri> GetOAuthUriAsync(AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken);

    /// <summary>
    /// Handles the OAuth callback request and extracts the authentication token.
    /// </summary>
    /// <param name="request">The HTTP request containing OAuth callback data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An authentication token if successful, null otherwise</returns>
    Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Logs out the current user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task LogoutAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves information about the current authenticated user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User information if authenticated, null otherwise</returns>
    Task<UserInfo?> GetUserInfoAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets the available authentication options.
    /// </summary>
    /// <returns>Array of supported authentication options</returns>
    AuthOption[] GetAuthOptions();

    /// <summary>
    /// Refreshes the current authentication token.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// The refreshed authentication token if successful; otherwise, null.
    /// </returns>
    Task<AuthToken?> RefreshAccessTokenAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets the current authentication token.
    /// </summary>
    /// <returns>The current authentication token if available, null otherwise</returns>
    AuthToken? GetCurrentToken();
}