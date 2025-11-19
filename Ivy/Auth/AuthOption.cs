using Ivy.Shared;

namespace Ivy.Auth;

public record AuthOption(AuthFlow Flow, string? Name = null, string? Id = null, Icons? Icon = null, object? Tag = null);

public enum AuthFlow
{
    EmailPassword,
    /// <summary>Magic link authentication via email</summary>
    MagicLink,
    /// <summary>One-time password authentication</summary>
    Otp,
    OAuth
}