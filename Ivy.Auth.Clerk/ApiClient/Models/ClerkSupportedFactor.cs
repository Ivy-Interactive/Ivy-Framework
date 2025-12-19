using System.Text.Json.Serialization;

namespace Ivy.Auth.Clerk.ApiClient.Models;

public class ClerkSupportedFactor
{
    [JsonPropertyName("strategy")]
    public required string Strategy { get; init; }
}
