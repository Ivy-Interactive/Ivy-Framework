namespace Ivy.Auth;

/// <summary> Represents an authentication token with optional refresh capabilities. </summary>
/// <param name="AccessToken">The access token</param>
/// <param name="RefreshToken">Optional refresh token for token renewal</param>
/// <param name="Tag">Optional additional data associated with the token</param>
public record AuthToken(
    string AccessToken,
    string? RefreshToken = null,
    object? Tag = null);
