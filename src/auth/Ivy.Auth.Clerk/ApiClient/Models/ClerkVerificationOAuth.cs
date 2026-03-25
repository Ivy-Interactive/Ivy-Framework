using System.Text.Json.Serialization;

namespace Ivy.Auth.Clerk.ApiClient.Models;

public class ClerkVerificationOAuth
{
    [JsonPropertyName("object")]
    public required string Object { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("strategy")]
    public required string Strategy { get; init; }

    [JsonPropertyName("attempts")]
    public int? Attempts { get; init; }

    [JsonPropertyName("expire_at")]
    public long? ExpireAt { get; init; }

    [JsonPropertyName("external_verification_redirect_url")]
    public string? ExternalVerificationRedirectUrl { get; init; }

    [JsonPropertyName("error")]
    public ClerkVerificationError? Error { get; init; }
}
