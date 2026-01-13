using System.Text.Json.Serialization;

namespace Ivy.Auth.Clerk.ApiClient.Models;

public class ClerkClient
{
    [JsonPropertyName("object")]
    public required string Object { get; init; }

    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("sessions")]
    public required List<ClerkSession> Sessions { get; init; }

    [JsonPropertyName("sign_in")]
    public ClerkSignInAttempt? SignIn { get; init; }

    [JsonPropertyName("sign_up")]
    public object? SignUp { get; init; }

    [JsonPropertyName("last_active_session_id")]
    public string? LastActiveSessionId { get; init; }

    [JsonPropertyName("last_authentication_strategy")]
    public string? LastAuthenticationStrategy { get; init; }

    [JsonPropertyName("cookie_expires_at")]
    public long? CookieExpiresAt { get; init; }

    [JsonPropertyName("captcha_bypass")]
    public bool CaptchaBypass { get; init; }

    [JsonPropertyName("created_at")]
    public long CreatedAt { get; init; }

    [JsonPropertyName("updated_at")]
    public long UpdatedAt { get; init; }
}
