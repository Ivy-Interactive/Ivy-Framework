using System.Text.Json.Serialization;

namespace Ivy.Auth.Clerk.ApiClient.Responses;

public class ClerkEnvironmentResponse
{
    [JsonPropertyName("auth_config")]
    public AuthConfig AuthConfig { get; set; } = new();

    [JsonPropertyName("display_config")]
    public DisplayConfig DisplayConfig { get; set; } = new();

    [JsonPropertyName("user_settings")]
    public UserSettings UserSettings { get; set; } = new();

    [JsonPropertyName("organization_settings")]
    public OrganizationSettings OrganizationSettings { get; set; } = new();

    [JsonPropertyName("fraud_settings")]
    public FraudSettings FraudSettings { get; set; } = new();

    [JsonPropertyName("commerce_settings")]
    public CommerceSettings CommerceSettings { get; set; } = new();

    [JsonPropertyName("api_keys_settings")]
    public ApiKeysSettings ApiKeysSettings { get; set; } = new();

    [JsonPropertyName("maintenance_mode")]
    public bool MaintenanceMode { get; set; }

    [JsonPropertyName("client_debug_mode")]
    public bool ClientDebugMode { get; set; }
}

public class AuthConfig
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("last_name")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("email_address")]
    public string EmailAddress { get; set; } = string.Empty;

    [JsonPropertyName("phone_number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    // Array of arrays of strategies
    [JsonPropertyName("identification_requirements")]
    public List<List<string>> IdentificationRequirements { get; set; } = new();

    [JsonPropertyName("identification_strategies")]
    public List<string> IdentificationStrategies { get; set; } = new();

    [JsonPropertyName("first_factors")]
    public List<string> FirstFactors { get; set; } = new();

    [JsonPropertyName("second_factors")]
    public List<string> SecondFactors { get; set; } = new();

    [JsonPropertyName("email_address_verification_strategies")]
    public List<string> EmailAddressVerificationStrategies { get; set; } = new();

    [JsonPropertyName("single_session_mode")]
    public bool SingleSessionMode { get; set; }

    [JsonPropertyName("enhanced_email_deliverability")]
    public bool EnhancedEmailDeliverability { get; set; }

    [JsonPropertyName("test_mode")]
    public bool TestMode { get; set; }

    [JsonPropertyName("cookieless_dev")]
    public bool CookielessDev { get; set; }

    [JsonPropertyName("url_based_session_syncing")]
    public bool UrlBasedSessionSyncing { get; set; }

    [JsonPropertyName("claimed_at")]
    public DateTimeOffset? ClaimedAt { get; set; }

    [JsonPropertyName("reverification")]
    public bool Reverification { get; set; }
}

public class DisplayConfig
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("instance_environment_type")]
    public string InstanceEnvironmentType { get; set; } = string.Empty;

    [JsonPropertyName("application_name")]
    public string ApplicationName { get; set; } = string.Empty;

    [JsonPropertyName("theme")]
    public Theme Theme { get; set; } = new();

    [JsonPropertyName("preferred_sign_in_strategy")]
    public string PreferredSignInStrategy { get; set; } = string.Empty;

    [JsonPropertyName("logo_image_url")]
    public string LogoImageUrl { get; set; } = string.Empty;

    [JsonPropertyName("favicon_image_url")]
    public string FaviconImageUrl { get; set; } = string.Empty;

    [JsonPropertyName("home_url")]
    public string HomeUrl { get; set; } = string.Empty;

    [JsonPropertyName("sign_in_url")]
    public string SignInUrl { get; set; } = string.Empty;

    [JsonPropertyName("sign_up_url")]
    public string SignUpUrl { get; set; } = string.Empty;

    [JsonPropertyName("user_profile_url")]
    public string UserProfileUrl { get; set; } = string.Empty;

    [JsonPropertyName("waitlist_url")]
    public string WaitlistUrl { get; set; } = string.Empty;

    [JsonPropertyName("after_sign_in_url")]
    public string AfterSignInUrl { get; set; } = string.Empty;

    [JsonPropertyName("after_sign_up_url")]
    public string AfterSignUpUrl { get; set; } = string.Empty;

    [JsonPropertyName("after_sign_out_one_url")]
    public string AfterSignOutOneUrl { get; set; } = string.Empty;

    [JsonPropertyName("after_sign_out_all_url")]
    public string AfterSignOutAllUrl { get; set; } = string.Empty;

    [JsonPropertyName("after_switch_session_url")]
    public string AfterSwitchSessionUrl { get; set; } = string.Empty;

    [JsonPropertyName("after_join_waitlist_url")]
    public string AfterJoinWaitlistUrl { get; set; } = string.Empty;

    [JsonPropertyName("organization_profile_url")]
    public string OrganizationProfileUrl { get; set; } = string.Empty;

    [JsonPropertyName("create_organization_url")]
    public string CreateOrganizationUrl { get; set; } = string.Empty;

    [JsonPropertyName("after_leave_organization_url")]
    public string AfterLeaveOrganizationUrl { get; set; } = string.Empty;

    [JsonPropertyName("after_create_organization_url")]
    public string AfterCreateOrganizationUrl { get; set; } = string.Empty;

    [JsonPropertyName("logo_link_url")]
    public string LogoLinkUrl { get; set; } = string.Empty;

    [JsonPropertyName("support_email")]
    public string? SupportEmail { get; set; }

    [JsonPropertyName("branded")]
    public bool Branded { get; set; }

    [JsonPropertyName("experimental_force_oauth_first")]
    public bool ExperimentalForceOauthFirst { get; set; }

    [JsonPropertyName("clerk_js_version")]
    public string ClerkJsVersion { get; set; } = string.Empty;

    [JsonPropertyName("show_devmode_warning")]
    public bool ShowDevmodeWarning { get; set; }

    [JsonPropertyName("google_one_tap_client_id")]
    public string? GoogleOneTapClientId { get; set; }

    [JsonPropertyName("help_url")]
    public string? HelpUrl { get; set; }

    [JsonPropertyName("privacy_policy_url")]
    public string? PrivacyPolicyUrl { get; set; }

    [JsonPropertyName("terms_url")]
    public string? TermsUrl { get; set; }

    [JsonPropertyName("logo_url")]
    public string? LogoUrl { get; set; }

    [JsonPropertyName("favicon_url")]
    public string? FaviconUrl { get; set; }

    [JsonPropertyName("logo_image")]
    public string? LogoImage { get; set; }

    [JsonPropertyName("favicon_image")]
    public string? FaviconImage { get; set; }

    [JsonPropertyName("captcha_public_key")]
    public string? CaptchaPublicKey { get; set; }

    [JsonPropertyName("captcha_widget_type")]
    public string? CaptchaWidgetType { get; set; }

    [JsonPropertyName("captcha_public_key_invisible")]
    public string? CaptchaPublicKeyInvisible { get; set; }

    [JsonPropertyName("captcha_provider")]
    public string? CaptchaProvider { get; set; }

    [JsonPropertyName("captcha_oauth_bypass")]
    public List<string> CaptchaOauthBypass { get; set; } = new();
}

public class Theme
{
    [JsonPropertyName("buttons")]
    public ThemeButtons Buttons { get; set; } = new();

    [JsonPropertyName("general")]
    public ThemeGeneral General { get; set; } = new();

    [JsonPropertyName("accounts")]
    public ThemeAccounts Accounts { get; set; } = new();
}

public class ThemeButtons
{
    [JsonPropertyName("font_color")]
    public string FontColor { get; set; } = string.Empty;

    [JsonPropertyName("font_family")]
    public string FontFamily { get; set; } = string.Empty;

    [JsonPropertyName("font_weight")]
    public string FontWeight { get; set; } = string.Empty;
}

public class ThemeGeneral
{
    [JsonPropertyName("color")]
    public string Color { get; set; } = string.Empty;

    [JsonPropertyName("padding")]
    public string Padding { get; set; } = string.Empty;

    [JsonPropertyName("box_shadow")]
    public string BoxShadow { get; set; } = string.Empty;

    [JsonPropertyName("font_color")]
    public string FontColor { get; set; } = string.Empty;

    [JsonPropertyName("font_family")]
    public string FontFamily { get; set; } = string.Empty;

    [JsonPropertyName("border_radius")]
    public string BorderRadius { get; set; } = string.Empty;

    [JsonPropertyName("background_color")]
    public string BackgroundColor { get; set; } = string.Empty;

    [JsonPropertyName("label_font_weight")]
    public string LabelFontWeight { get; set; } = string.Empty;
}

public class ThemeAccounts
{
    [JsonPropertyName("background_color")]
    public string BackgroundColor { get; set; } = string.Empty;
}

public class UserSettings
{
    // keys: email_address, phone_number, username, web3_wallet, first_name, last_name, password, authenticator_app, ticket, backup_code, passkey
    [JsonPropertyName("attributes")]
    public Dictionary<string, AttributeSettings> Attributes { get; set; } = new();

    [JsonPropertyName("sign_in")]
    public SignInSettings SignIn { get; set; } = new();

    [JsonPropertyName("sign_up")]
    public SignUpSettings SignUp { get; set; } = new();

    [JsonPropertyName("restrictions")]
    public RestrictionsSettings Restrictions { get; set; } = new();

    [JsonPropertyName("username_settings")]
    public UsernameSettings UsernameSettings { get; set; } = new();

    [JsonPropertyName("actions")]
    public UserActions Actions { get; set; } = new();

    [JsonPropertyName("attack_protection")]
    public AttackProtectionSettings AttackProtection { get; set; } = new();

    [JsonPropertyName("passkey_settings")]
    public PasskeySettings PasskeySettings { get; set; } = new();

    // keys like oauth_google, oauth_linkedin_oidc, etc.
    [JsonPropertyName("social")]
    public Dictionary<string, SocialProviderSettings> Social { get; set; } = new();

    [JsonPropertyName("password_settings")]
    public PasswordSettings PasswordSettings { get; set; } = new();

    [JsonPropertyName("saml")]
    public ToggleSetting Saml { get; set; } = new();

    [JsonPropertyName("enterprise_sso")]
    public ToggleSetting EnterpriseSso { get; set; } = new();
}

public class AttributeSettings
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("required")]
    public bool Required { get; set; }

    [JsonPropertyName("used_for_first_factor")]
    public bool UsedForFirstFactor { get; set; }

    [JsonPropertyName("first_factors")]
    public List<string> FirstFactors { get; set; } = new();

    [JsonPropertyName("used_for_second_factor")]
    public bool UsedForSecondFactor { get; set; }

    [JsonPropertyName("second_factors")]
    public List<string> SecondFactors { get; set; } = new();

    [JsonPropertyName("verifications")]
    public List<string> Verifications { get; set; } = new();

    [JsonPropertyName("verify_at_sign_up")]
    public bool VerifyAtSignUp { get; set; }
}

public class SignInSettings
{
    [JsonPropertyName("second_factor")]
    public RequiredToggle SecondFactor { get; set; } = new();
}

public class RequiredToggle
{
    [JsonPropertyName("required")]
    public bool Required { get; set; }
}

public class SignUpSettings
{
    [JsonPropertyName("captcha_enabled")]
    public bool CaptchaEnabled { get; set; }

    [JsonPropertyName("captcha_widget_type")]
    public string? CaptchaWidgetType { get; set; }

    [JsonPropertyName("custom_action_required")]
    public bool CustomActionRequired { get; set; }

    [JsonPropertyName("progressive")]
    public bool Progressive { get; set; }

    [JsonPropertyName("mode")]
    public string Mode { get; set; } = string.Empty;

    [JsonPropertyName("legal_consent_enabled")]
    public bool LegalConsentEnabled { get; set; }
}

public class RestrictionsSettings
{
    [JsonPropertyName("allowlist")]
    public ToggleSetting Allowlist { get; set; } = new();

    [JsonPropertyName("blocklist")]
    public ToggleSetting Blocklist { get; set; } = new();

    [JsonPropertyName("allowlist_blocklist_disabled_on_sign_in")]
    public ToggleSetting AllowlistBlocklistDisabledOnSignIn { get; set; } = new();

    [JsonPropertyName("block_email_subaddresses")]
    public ToggleSetting BlockEmailSubaddresses { get; set; } = new();

    [JsonPropertyName("block_disposable_email_domains")]
    public ToggleSetting BlockDisposableEmailDomains { get; set; } = new();
}

public class ToggleSetting
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }
}

public class UsernameSettings
{
    [JsonPropertyName("min_length")]
    public int MinLength { get; set; }

    [JsonPropertyName("max_length")]
    public int MaxLength { get; set; }

    [JsonPropertyName("allow_extended_special_characters")]
    public bool AllowExtendedSpecialCharacters { get; set; }
}

public class UserActions
{
    [JsonPropertyName("delete_self")]
    public bool DeleteSelf { get; set; }

    [JsonPropertyName("create_organization")]
    public bool CreateOrganization { get; set; }

    [JsonPropertyName("create_organizations_limit")]
    public int? CreateOrganizationsLimit { get; set; }
}

public class AttackProtectionSettings
{
    [JsonPropertyName("user_lockout")]
    public UserLockout UserLockout { get; set; } = new();

    [JsonPropertyName("pii")]
    public ToggleSetting Pii { get; set; } = new();

    [JsonPropertyName("email_link")]
    public EmailLinkProtection EmailLink { get; set; } = new();

    [JsonPropertyName("enumeration_protection")]
    public ToggleSetting EnumerationProtection { get; set; } = new();
}

public class UserLockout
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("max_attempts")]
    public int MaxAttempts { get; set; }

    [JsonPropertyName("duration_in_minutes")]
    public int DurationInMinutes { get; set; }
}

public class EmailLinkProtection
{
    [JsonPropertyName("require_same_client")]
    public bool RequireSameClient { get; set; }
}

public class PasskeySettings
{
    [JsonPropertyName("allow_autofill")]
    public bool AllowAutofill { get; set; }

    [JsonPropertyName("show_sign_in_button")]
    public bool ShowSignInButton { get; set; }
}

public class SocialProviderSettings
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("required")]
    public bool Required { get; set; }

    [JsonPropertyName("authenticatable")]
    public bool Authenticatable { get; set; }

    [JsonPropertyName("block_email_subaddresses")]
    public bool BlockEmailSubaddresses { get; set; }

    [JsonPropertyName("strategy")]
    public string Strategy { get; set; } = string.Empty;

    [JsonPropertyName("not_selectable")]
    public bool NotSelectable { get; set; }

    [JsonPropertyName("deprecated")]
    public bool Deprecated { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("logo_url")]
    public string LogoUrl { get; set; } = string.Empty;
}

public class PasswordSettings
{
    [JsonPropertyName("disable_hibp")]
    public bool DisableHibp { get; set; }

    [JsonPropertyName("min_length")]
    public int MinLength { get; set; }

    [JsonPropertyName("max_length")]
    public int MaxLength { get; set; }

    [JsonPropertyName("require_special_char")]
    public bool RequireSpecialChar { get; set; }

    [JsonPropertyName("require_numbers")]
    public bool RequireNumbers { get; set; }

    [JsonPropertyName("require_uppercase")]
    public bool RequireUppercase { get; set; }

    [JsonPropertyName("require_lowercase")]
    public bool RequireLowercase { get; set; }

    [JsonPropertyName("show_zxcvbn")]
    public bool ShowZxcvbn { get; set; }

    [JsonPropertyName("min_zxcvbn_strength")]
    public int MinZxcvbnStrength { get; set; }

    [JsonPropertyName("enforce_hibp_on_sign_in")]
    public bool EnforceHibpOnSignIn { get; set; }

    [JsonPropertyName("allowed_special_characters")]
    public string AllowedSpecialCharacters { get; set; } = string.Empty;
}

public class OrganizationSettings
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("max_allowed_memberships")]
    public int MaxAllowedMemberships { get; set; }

    [JsonPropertyName("actions")]
    public OrganizationActions Actions { get; set; } = new();

    [JsonPropertyName("domains")]
    public OrganizationDomains Domains { get; set; } = new();

    [JsonPropertyName("slug")]
    public OrganizationSlug Slug { get; set; } = new();

    [JsonPropertyName("creator_role")]
    public string CreatorRole { get; set; } = string.Empty;

    [JsonPropertyName("force_organization_selection")]
    public bool ForceOrganizationSelection { get; set; }
}

public class OrganizationActions
{
    [JsonPropertyName("admin_delete")]
    public bool AdminDelete { get; set; }
}

public class OrganizationDomains
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("enrollment_modes")]
    public List<string> EnrollmentModes { get; set; } = new();

    [JsonPropertyName("default_role")]
    public string DefaultRole { get; set; } = string.Empty;
}

public class OrganizationSlug
{
    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; }
}

public class FraudSettings
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("native")]
    public FraudNative Native { get; set; } = new();
}

public class FraudNative
{
    [JsonPropertyName("device_attestation_mode")]
    public string DeviceAttestationMode { get; set; } = string.Empty;
}

public class CommerceSettings
{
    [JsonPropertyName("billing")]
    public BillingSettings Billing { get; set; } = new();
}

public class BillingSettings
{
    [JsonPropertyName("stripe_publishable_key")]
    public string? StripePublishableKey { get; set; }

    [JsonPropertyName("free_trial_requires_payment_method")]
    public bool FreeTrialRequiresPaymentMethod { get; set; }

    [JsonPropertyName("user")]
    public BillingToggle User { get; set; } = new();

    [JsonPropertyName("organization")]
    public BillingToggle Organization { get; set; } = new();
}

public class BillingToggle
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("has_paid_plans")]
    public bool HasPaidPlans { get; set; }
}

public class ApiKeysSettings
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("user_api_keys_enabled")]
    public bool UserApiKeysEnabled { get; set; }

    [JsonPropertyName("show_in_user_profile")]
    public bool ShowInUserProfile { get; set; }

    [JsonPropertyName("orgs_api_keys_enabled")]
    public bool OrgsApiKeysEnabled { get; set; }

    [JsonPropertyName("show_in_org_profile")]
    public bool ShowInOrgProfile { get; set; }
}