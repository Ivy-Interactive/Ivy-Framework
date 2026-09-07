using System.Text.Json.Serialization;

namespace Ivy.Auth.Clerk.ApiClient.Responses;

public class ClerkOAuthAccessTokenResponse
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = default!;

    [JsonPropertyName("external_account_id")]
    public string ExternalAccountId { get; set; } = default!;

    [JsonPropertyName("provider_user_id")]
    public string? ProviderUserId { get; set; }

    [JsonPropertyName("provider")]
    public string Provider { get; set; } = default!;

    [JsonPropertyName("token")]
    public string Token { get; set; } = default!;

    [JsonPropertyName("expires_at")]
    public long? ExpiresAt { get; set; }

    [JsonPropertyName("public_metadata")]
    public Dictionary<string, object>? PublicMetadata { get; set; }

    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("scopes")]
    public string[]? Scopes { get; set; }

    [JsonPropertyName("token_secret")]
    public string? TokenSecret { get; set; }
}
