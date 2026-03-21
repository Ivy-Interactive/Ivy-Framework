using System.Text.Json.Serialization;
using Ivy.Auth.Clerk.ApiClient.Models;

namespace Ivy.Auth.Clerk.ApiClient.Responses;

public class ClerkSignUpResponse
{
    [JsonPropertyName("response")]
    public ClerkSignUpAttempt? Response { get; set; }

    [JsonPropertyName("client")]
    public ClerkClient? Client { get; set; }
}
