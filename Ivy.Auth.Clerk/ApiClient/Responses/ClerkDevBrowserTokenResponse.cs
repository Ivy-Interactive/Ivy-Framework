using System.Text.Json.Serialization;

namespace Ivy.Auth.Clerk.ApiClient.Responses;

public class ClerkDevBrowserTokenResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("instance_id")]
    public string InstanceId { get; set; } = string.Empty;

    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("client_id")]
    public string? ClientId { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("home_origin")]
    public string? HomeOrigin { get; set; }
}
