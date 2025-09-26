using Ivy.Shared;

namespace Ivy.Auth;

/// <summary>
/// Represents an authentication option available to users.
/// </summary>
/// <param name="Flow">The authentication flow type</param>
/// <param name="Name">Display name for the authentication option</param>
/// <param name="Id">Unique identifier for the authentication option</param>
/// <param name="Icon">Icon to display for the authentication option</param>
/// <param name="Tag">Additional data associated with the authentication option</param>
public record AuthOption(AuthFlow Flow, string? Name = null, string? Id = null, Icons? Icon = null, object? Tag = null);

/// <summary>
/// Defines the available authentication flow types.
/// </summary>
public enum AuthFlow
{
    /// <summary>Email and password authentication</summary>
    EmailPassword,
    /// <summary>Magic link authentication via email</summary>
    MagicLink,
    /// <summary>One-time password authentication</summary>
    Otp,
    /// <summary>OAuth authentication with external providers</summary>
    OAuth
}