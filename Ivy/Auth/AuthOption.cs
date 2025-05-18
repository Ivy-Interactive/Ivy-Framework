using Ivy.Shared;

namespace Ivy.Auth;

public record AuthOption(AuthFlow Flow, string? Name = null, string? Id = null, Icons? Icon = null);
// Comment here for PR auto check purposes - working variant
public enum AuthFlow
{
    EmailPassword,
    MagicLink,
    Otp,
    OAuth
}