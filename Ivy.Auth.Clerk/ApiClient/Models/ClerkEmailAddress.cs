using System.Text.Json.Serialization;

namespace Ivy.Auth.Clerk.ApiClient.Models;

public class ClerkEmailAddress
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    [JsonPropertyName("object")]
    public string Object { get; set; } = default!;

    [JsonPropertyName("email_address")]
    public string EmailAddress { get; set; } = default!;

    [JsonPropertyName("reserved")]
    public bool Reserved { get; set; }

    [JsonPropertyName("verification")]
    public ClerkVerification Verification { get; set; } = default!;

    [JsonPropertyName("linked_to")]
    public List<ClerkLinkedTo> LinkedTo { get; set; } = default!;

    [JsonPropertyName("matches_sso_connection")]
    public bool MatchesSsoConnection { get; set; }

    [JsonPropertyName("created_at")]
    public long CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public long UpdatedAt { get; set; }
}
