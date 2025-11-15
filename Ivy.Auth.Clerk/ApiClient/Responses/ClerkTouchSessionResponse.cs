using System.Text.Json.Serialization;

namespace Ivy.Auth.Clerk.ApiClient.Responses;

public class ClerkTouchSessionResponse
{
    [JsonPropertyName("response")]
    public ClerkSession Response { get; set; } = default!;

    [JsonPropertyName("client")]
    public ClerkClient Client { get; set; } = default!;
}
