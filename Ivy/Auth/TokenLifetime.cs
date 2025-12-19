namespace Ivy.Auth;

public record TokenLifetime(DateTimeOffset? Expires = null, DateTimeOffset? NotBefore = null);