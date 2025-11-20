using System.Text.Json.Serialization;
using Ivy.Auth.Clerk.ApiClient.Models;

namespace Ivy.Auth.Clerk.ApiClient.Responses;

public class ClerkTokenResponse
{
    [JsonPropertyName("object")]
    public string? Object { get; set; }

    [JsonPropertyName("jwt")]
    public string? Jwt { get; set; }
}
