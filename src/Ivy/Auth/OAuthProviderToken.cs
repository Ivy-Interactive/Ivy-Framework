// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Represents an OAuth access token from an external OAuth provider (Google, GitHub, etc.)
/// obtained through an authentication provider like Clerk or Auth0.
/// </summary>
public record OAuthProviderToken(
    string Provider,
    string AccessToken,
    string[]? Scopes = null,
    string? RefreshToken = null,
    DateTimeOffset? ExpiresAt = null,
    Dictionary<string, object>? PublicMetadata = null,
    string? Label = null);
