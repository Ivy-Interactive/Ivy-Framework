using System.Text.Json.Serialization;

namespace Ivy.Auth.Clerk.ApiClient;

public class ClerkToken
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = default!;

    [JsonPropertyName("jwt")]
    public string Jwt { get; set; } = default!;
}
