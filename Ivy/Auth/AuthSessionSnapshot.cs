namespace Ivy.Auth;

public readonly struct AuthSessionSnapshot
{
    public readonly AuthToken? AuthToken { get; init; }
    public readonly string? AuthSessionData { get; init; }
}
