using System.Text.Json.Serialization;
using Ivy.Auth.Clerk.ApiClient.Models;

namespace Ivy.Auth.Clerk.ApiClient.Responses;

public class ClerkSessionResponse
{
    [JsonPropertyName("response")]
    public ClerkSession Response { get; set; } = default!;

    [JsonPropertyName("client")]
    public ClerkClient Client { get; set; } = default!;
}
