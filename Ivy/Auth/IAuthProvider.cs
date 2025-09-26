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
    /// <returns>An authentication token if successful, null otherwise</returns>
    Task<AuthToken?> LoginAsync(string email, string password);

    /// <summary>
    /// Logs out a user by invalidating their JWT token.
    /// </summary>
    /// <param name="jwt">The JWT token to invalidate</param>
    Task LogoutAsync(string jwt);

    /// <summary>
    /// Refreshes an expired or expiring JWT token.
    /// </summary>
    /// <param name="jwt">The current authentication token</param>
    /// <returns>A new authentication token if successful, null otherwise</returns>
    Task<AuthToken?> RefreshJwtAsync(AuthToken jwt);

    /// <summary>
    /// Validates whether a JWT token is still valid.
    /// </summary>
    /// <param name="jwt">The JWT token to validate</param>
    /// <returns>True if the token is valid, false otherwise</returns>
    Task<bool> ValidateJwtAsync(string jwt);

    /// <summary>
    /// Retrieves user information from a valid JWT token.
    /// </summary>
    /// <param name="jwt">The JWT token</param>
    /// <returns>User information if successful, null otherwise</returns>
    Task<UserInfo?> GetUserInfoAsync(string jwt);

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
    /// <returns>The OAuth authorization URI</returns>
    Task<Uri> GetOAuthUriAsync(AuthOption option, WebhookEndpoint callback);

    /// <summary>
    /// Handles the OAuth callback request and extracts the authentication token.
    /// </summary>
    /// <param name="request">The HTTP request containing OAuth callback data</param>
    /// <returns>An authentication token if successful, null otherwise</returns>
    Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request);

    /// <summary>
    /// Sets the HTTP context for the auth provider.
    /// </summary>
    /// <param name="context">The HTTP context</param>
    void SetHttpContext(HttpContext context)
    {
    }
}