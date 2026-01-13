using System.Text.Json.Serialization;

namespace Ivy.Auth.Clerk.ApiClient.Models;

public class ClerkUser
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    [JsonPropertyName("object")]
    public string Object { get; set; } = default!;

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }

    [JsonPropertyName("locale")]
    public string? Locale { get; set; }

    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("has_image")]
    public bool HasImage { get; set; }

    [JsonPropertyName("primary_email_address_id")]
    public string? PrimaryEmailAddressId { get; set; }

    [JsonPropertyName("primary_phone_number_id")]
    public string? PrimaryPhoneNumberId { get; set; }

    [JsonPropertyName("primary_web3_wallet_id")]
    public string? PrimaryWeb3WalletId { get; set; }

    [JsonPropertyName("password_enabled")]
    public bool PasswordEnabled { get; set; }

    [JsonPropertyName("two_factor_enabled")]
    public bool TwoFactorEnabled { get; set; }

    [JsonPropertyName("totp_enabled")]
    public bool TotpEnabled { get; set; }

    [JsonPropertyName("backup_code_enabled")]
    public bool BackupCodeEnabled { get; set; }

    [JsonPropertyName("email_addresses")]
    public List<ClerkEmailAddress> EmailAddresses { get; set; } = default!;

    [JsonPropertyName("phone_numbers")]
    public List<object> PhoneNumbers { get; set; } = default!;

    [JsonPropertyName("web3_wallets")]
    public List<object> Web3Wallets { get; set; } = default!;

    [JsonPropertyName("passkeys")]
    public List<object> Passkeys { get; set; } = default!;

    [JsonPropertyName("external_accounts")]
    public List<ClerkExternalAccount> ExternalAccounts { get; set; } = default!;

    [JsonPropertyName("saml_accounts")]
    public List<object> SamlAccounts { get; set; } = default!;

    [JsonPropertyName("enterprise_accounts")]
    public List<object> EnterpriseAccounts { get; set; } = default!;

    [JsonPropertyName("password_last_updated_at")]
    public long? PasswordLastUpdatedAt { get; set; }

    [JsonPropertyName("public_metadata")]
    public Dictionary<string, object> PublicMetadata { get; set; } = default!;

    [JsonPropertyName("unsafe_metadata")]
    public Dictionary<string, object> UnsafeMetadata { get; set; } = default!;

    [JsonPropertyName("external_id")]
    public string? ExternalId { get; set; }

    [JsonPropertyName("last_sign_in_at")]
    public long LastSignInAt { get; set; }

    [JsonPropertyName("banned")]
    public bool Banned { get; set; }

    [JsonPropertyName("locked")]
    public bool Locked { get; set; }

    [JsonPropertyName("lockout_expires_in_seconds")]
    public long? LockoutExpiresInSeconds { get; set; }

    [JsonPropertyName("verification_attempts_remaining")]
    public int VerificationAttemptsRemaining { get; set; }

    [JsonPropertyName("created_at")]
    public long CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public long UpdatedAt { get; set; }

    [JsonPropertyName("delete_self_enabled")]
    public bool DeleteSelfEnabled { get; set; }

    [JsonPropertyName("create_organization_enabled")]
    public bool CreateOrganizationEnabled { get; set; }

    [JsonPropertyName("last_active_at")]
    public long LastActiveAt { get; set; }

    [JsonPropertyName("mfa_enabled_at")]
    public long? MfaEnabledAt { get; set; }

    [JsonPropertyName("mfa_disabled_at")]
    public long? MfaDisabledAt { get; set; }

    [JsonPropertyName("legal_accepted_at")]
    public long? LegalAcceptedAt { get; set; }

    [JsonPropertyName("profile_image_url")]
    public string? ProfileImageUrl { get; set; }

    [JsonPropertyName("organization_memberships")]
    public List<object> OrganizationMemberships { get; set; } = default!;
}
