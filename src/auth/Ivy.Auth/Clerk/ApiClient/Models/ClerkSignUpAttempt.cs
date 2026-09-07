using System.Text.Json.Serialization;

namespace Ivy.Auth.Clerk.ApiClient.Models;

public class ClerkSignUpAttempt
{
    [JsonPropertyName("object")]
    public required string Object { get; init; }

    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("required_fields")]
    public List<string> RequiredFields { get; init; } = [];

    [JsonPropertyName("optional_fields")]
    public List<string> OptionalFields { get; init; } = [];

    [JsonPropertyName("missing_fields")]
    public List<string> MissingFields { get; init; } = [];

    [JsonPropertyName("unverified_fields")]
    public List<string> UnverifiedFields { get; init; } = [];

    [JsonPropertyName("verifications")]
    public ClerkSignUpVerifications? Verifications { get; init; }

    [JsonPropertyName("username")]
    public string? Username { get; init; }

    [JsonPropertyName("email_address")]
    public string? EmailAddress { get; init; }

    [JsonPropertyName("phone_number")]
    public string? PhoneNumber { get; init; }

    [JsonPropertyName("web3_wallet")]
    public string? Web3Wallet { get; init; }

    [JsonPropertyName("password_enabled")]
    public bool PasswordEnabled { get; init; }

    [JsonPropertyName("first_name")]
    public string? FirstName { get; init; }

    [JsonPropertyName("last_name")]
    public string? LastName { get; init; }

    [JsonPropertyName("unsafe_metadata")]
    public Dictionary<string, object>? UnsafeMetadata { get; init; }

    [JsonPropertyName("public_metadata")]
    public Dictionary<string, object>? PublicMetadata { get; init; }

    [JsonPropertyName("custom_action")]
    public bool CustomAction { get; init; }

    [JsonPropertyName("external_id")]
    public string? ExternalId { get; init; }

    [JsonPropertyName("created_session_id")]
    public string? CreatedSessionId { get; init; }

    [JsonPropertyName("created_user_id")]
    public string? CreatedUserId { get; init; }

    [JsonPropertyName("abandon_at")]
    public long AbandonAt { get; init; }

    [JsonPropertyName("legal_accepted_at")]
    public long? LegalAcceptedAt { get; init; }
}

public class ClerkSignUpVerifications
{
    [JsonPropertyName("email_address")]
    public object? EmailAddress { get; init; }

    [JsonPropertyName("phone_number")]
    public object? PhoneNumber { get; init; }

    [JsonPropertyName("web3_wallet")]
    public object? Web3Wallet { get; init; }

    [JsonPropertyName("external_account")]
    public ClerkVerificationOAuth? ExternalAccount { get; init; }
}
