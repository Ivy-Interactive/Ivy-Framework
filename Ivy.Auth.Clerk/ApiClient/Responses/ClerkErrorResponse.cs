using System.Text.Json.Serialization;

namespace Ivy.Auth.Clerk.ApiClient.Responses;

public class ClerkErrorResponse
{
    [JsonPropertyName("errors")]
    public List<ClerkError> Errors { get; set; } = [];

    [JsonPropertyName("clerk_trace_id")]
    public string ClerkTraceId { get; set; } = string.Empty;
}

public class ClerkError
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("long_message")]
    public string LongMessage { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;
}
