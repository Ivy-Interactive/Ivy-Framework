using System.Text.Json.Serialization;

namespace Ivy.Auth.Clerk.ApiClient.Models;

public class ClerkVerification
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = default!;

    [JsonPropertyName("status")]
    public string Status { get; set; } = default!;

    [JsonPropertyName("strategy")]
    public string Strategy { get; set; } = default!;

    [JsonPropertyName("attempts")]
    public int? Attempts { get; set; }

    [JsonPropertyName("expire_at")]
    public long? ExpireAt { get; set; }
}
