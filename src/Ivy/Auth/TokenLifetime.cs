// ReSharper disable once CheckNamespace
namespace Ivy;

public record TokenLifetime(DateTimeOffset? Expires = null, DateTimeOffset? NotBefore = null);
