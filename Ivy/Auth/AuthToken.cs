namespace Ivy.Auth;

public record AuthToken(
    string AccessToken,
    string? RefreshToken = null,
    object? Tag = null);
