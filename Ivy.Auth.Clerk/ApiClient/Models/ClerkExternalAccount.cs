using System.Text.Json.Serialization;

namespace Ivy.Auth.Clerk.ApiClient.Models;

public class ClerkExternalAccount
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = default!;

    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    [JsonPropertyName("provider")]
    public string Provider { get; set; } = default!;

    [JsonPropertyName("identification_id")]
    public string IdentificationId { get; set; } = default!;

    [JsonPropertyName("provider_user_id")]
    public string ProviderUserId { get; set; } = default!;

    [JsonPropertyName("approved_scopes")]
    public string ApprovedScopes { get; set; } = default!;

    [JsonPropertyName("email_address")]
    public string EmailAddress { get; set; } = default!;

    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = default!;

    [JsonPropertyName("last_name")]
    public string LastName { get; set; } = default!;

    [JsonPropertyName("avatar_url")]
    public string AvatarUrl { get; set; } = default!;

    [JsonPropertyName("image_url")]
    public string ImageUrl { get; set; } = default!;

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("phone_number")]
    public string? PhoneNumber { get; set; }

    [JsonPropertyName("public_metadata")]
    public Dictionary<string, object> PublicMetadata { get; set; } = default!;

    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("created_at")]
    public long CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public long UpdatedAt { get; set; }

    [JsonPropertyName("verification")]
    public ClerkVerification Verification { get; set; } = default!;
}
