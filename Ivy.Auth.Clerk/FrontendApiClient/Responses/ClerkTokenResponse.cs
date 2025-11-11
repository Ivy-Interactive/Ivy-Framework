using System.Text.Json.Serialization;

namespace Ivy.Auth.Clerk.FrontendApiClient.Responses;

public class ClerkTokenResponse
{
    [JsonPropertyName("object")]
    public string? Object { get; set; }

    [JsonPropertyName("jwt")]
    public string? Jwt { get; set; }
}
