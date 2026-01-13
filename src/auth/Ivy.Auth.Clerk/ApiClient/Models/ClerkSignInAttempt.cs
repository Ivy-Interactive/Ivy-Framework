using System.Text.Json.Serialization;

namespace Ivy.Auth.Clerk.ApiClient.Models;

public class ClerkSignInAttempt
{
    [JsonPropertyName("object")]
    public required string Object { get; init; }

    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("supported_identifiers")]
    public List<string> SupportedIdentifiers { get; init; } = [];

    [JsonPropertyName("supported_first_factors")]
    public List<ClerkSupportedFactor> SupportedFirstFactors { get; init; } = [];

    [JsonPropertyName("supported_second_factors")]
    public List<ClerkSupportedFactor>? SupportedSecondFactors { get; init; }

    [JsonPropertyName("first_factor_verification")]
    public ClerkVerificationOAuth? FirstFactorVerification { get; init; }

    [JsonPropertyName("second_factor_verification")]
    public ClerkVerificationOAuth? SecondFactorVerification { get; init; }

    [JsonPropertyName("identifier")]
    public string? Identifier { get; init; }

    [JsonPropertyName("user_data")]
    public object? UserData { get; init; }

    [JsonPropertyName("created_session_id")]
    public string? CreatedSessionId { get; init; }

    [JsonPropertyName("abandon_at")]
    public long AbandonAt { get; init; }

    [JsonPropertyName("locale")]
    public string? Locale { get; init; }
}
