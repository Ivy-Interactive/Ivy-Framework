using Ivy.Auth;

namespace Ivy.Core.Auth;

public static class AuthSessionExtensions
{
#if DEBUG
    internal static CheckedAuthSessionBuilder WithCheckedAccess(this IAuthSession authSession)
        => new(authSession);
#endif
}