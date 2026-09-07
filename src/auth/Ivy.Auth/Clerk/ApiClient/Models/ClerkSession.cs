using System.Text.Json.Serialization;

namespace Ivy.Auth.Clerk.ApiClient.Models;

public class ClerkSession
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = default!;

    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    [JsonPropertyName("status")]
    public string Status { get; set; } = default!;

    [JsonPropertyName("expire_at")]
    public long? ExpireAt { get; set; }

    [JsonPropertyName("abandon_at")]
    public long? AbandonAt { get; set; }

    [JsonPropertyName("last_active_at")]
    public long? LastActiveAt { get; set; }

    [JsonPropertyName("last_active_organization_id")]
    public string? LastActiveOrganizationId { get; set; }

    [JsonPropertyName("actor")]
    public object? Actor { get; set; }

    [JsonPropertyName("user")]
    public ClerkUser User { get; set; } = default!;

    [JsonPropertyName("public_user_data")]
    public ClerkPublicUserData PublicUserData { get; set; } = default!;

    [JsonPropertyName("factor_verification_age")]
    public int[] FactorVerificationAge { get; set; } = default!;

    [JsonPropertyName("created_at")]
    public long CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public long UpdatedAt { get; set; }

    [JsonPropertyName("last_active_token")]
    public ClerkToken LastActiveToken { get; set; } = default!;
}
