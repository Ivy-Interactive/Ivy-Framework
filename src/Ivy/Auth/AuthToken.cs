// ReSharper disable once CheckNamespace
namespace Ivy;

public record AuthToken(
    string AccessToken,
    string? RefreshToken = null,
    string? Tag = null);
