using System.Text.Json.Serialization;

namespace Ivy.Auth.Clerk.FrontendApiClient.Responses;

public class ClerkClientResponse
{
    [JsonPropertyName("response")]
    public ClerkClient? Response { get; set; }

    [JsonPropertyName("client")]
    public object? Client { get; set; }
}

public class ClerkClient
{
    [JsonPropertyName("object")]
    public string? Object { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("sessions")]
    public List<object>? Sessions { get; set; }

    [JsonPropertyName("sign_in")]
    public object? SignIn { get; set; }

    [JsonPropertyName("sign_up")]
    public object? SignUp { get; set; }

    [JsonPropertyName("last_active_session_id")]
    public string? LastActiveSessionId { get; set; }

    [JsonPropertyName("last_authentication_strategy")]
    public string? LastAuthenticationStrategy { get; set; }

    [JsonPropertyName("cookie_expires_at")]
    public long? CookieExpiresAt { get; set; }

    [JsonPropertyName("captcha_bypass")]
    public bool CaptchaBypass { get; set; }

    [JsonPropertyName("created_at")]
    public long CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public long UpdatedAt { get; set; }
}
