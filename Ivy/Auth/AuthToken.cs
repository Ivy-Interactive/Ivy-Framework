namespace Ivy.Auth;

public record AuthToken(
    string Jwt,
    string? RefreshToken = null,
    DateTimeOffset? ExpiresAt = null,
    object? Tag = null);
