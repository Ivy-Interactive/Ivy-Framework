using System.Text.Json.Serialization;

namespace Ivy.Auth.Clerk.ApiClient.Models;

public class ClerkPublicUserData
{
    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }

    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("has_image")]
    public bool HasImage { get; set; }

    [JsonPropertyName("identifier")]
    public string Identifier { get; set; } = default!;

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("profile_image_url")]
    public string? ProfileImageUrl { get; set; }
}
