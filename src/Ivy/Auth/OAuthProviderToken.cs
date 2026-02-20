// ReSharper disable once CheckNamespace
namespace Ivy;

public record OAuthProviderToken(
    OAuthProvider Provider,
    string AccessToken,
    string[]? Scopes = null,
    string? RefreshToken = null,
    DateTimeOffset? ExpiresAt = null,
    Dictionary<string, object>? PublicMetadata = null,
    string? Label = null);
