using System.Text.Json.Serialization;
using Ivy.Auth.Clerk.ApiClient.Models;

namespace Ivy.Auth.Clerk.ApiClient.Responses;

public class ClerkErrorResponse
{
    [JsonPropertyName("errors")]
    public List<ClerkError> Errors { get; set; } = [];

    [JsonPropertyName("clerk_trace_id")]
    public string ClerkTraceId { get; set; } = string.Empty;
}
