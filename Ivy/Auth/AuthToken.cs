namespace Ivy.Auth;

/// <summary>
/// Represents an authentication token with optional refresh capabilities.
/// </summary>
/// <param name="Jwt">The JWT access token</param>
/// <param name="RefreshToken">Optional refresh token for token renewal</param>
/// <param name="ExpiresAt">Optional expiration timestamp</param>
/// <param name="Tag">Optional additional data associated with the token</param>
public record AuthToken(
    string Jwt,
    string? RefreshToken = null,
    DateTimeOffset? ExpiresAt = null,
    object? Tag = null);
