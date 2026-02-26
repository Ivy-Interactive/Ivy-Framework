// ReSharper disable once CheckNamespace
namespace Ivy;

public record OAuthProviderToken(
    OAuthProvider Provider,
    AuthToken AuthToken,
    string[]? Scopes = null,
    DateTimeOffset? ExpiresAt = null,
    Dictionary<string, object>? PublicMetadata = null,
    string? Label = null);
