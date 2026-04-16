---
searchHints:
  - authentication
  - login
  - security
  - auth
  - oauth
  - identity
---

# Authentication Overview

<Ingress>
Secure your Ivy application with integrated authentication providers including Auth0, Clerk, Supabase, Authelia, and Microsoft Entra ID.
</Ingress>

The `ivy auth add` command lets you add and configure authentication in your Ivy project. Ivy supports multiple providers and automatically updates your project setup to integrate them.

## Adding an Authentication Provider

To add authentication to your Ivy project, run:

```terminal
>ivy auth add
```

If you run this command without additional options, Ivy will guide you through an interactive setup:

1. **Select a Provider**: Choose from the available authentication providers
2. **Configure the Provider**: Enter the necessary configuration details (for example, domain and client ID for Auth0, or project URL and API key for Supabase)
3. **Project Setup**: Ivy updates your [Program.cs](../../02_Concepts/01_Program.md) and stores sensitive values in [.NET user secrets](../../02_Concepts/14_Secrets.md), so your project is ready to use authentication.

### Command Options

`--provider <PROVIDER>` or `-p <PROVIDER>` - Specify the authentication provider directly:

```terminal
>ivy auth add --provider Auth0
```

Available providers: `Auth0`, `Clerk`, `Supabase`, `MicrosoftEntra`, `Authelia`, `Basic`

`--connection-string <CONNECTION_STRING>` - Provide provider-specific configuration using connection string syntax. When used with `--provider`, this option allows you pass all required configuration inline, rather than being prompted interactively during setup:

```terminal
>ivy auth add --provider Auth0 --connection-string YourConnectionString
```

`--verbose` or `-v` - Enable verbose output for detailed logging:

```terminal
>ivy auth add --verbose
```

### How Ivy Updates Program.cs

Ivy automatically updates your [Program.cs](../../02_Concepts/01_Program.md) to configure authentication. Here are a few examples of code that it may add:

**Basic Auth**

```csharp
server.UseAuth<BasicAuthProvider>();
```

**Auth0**

```csharp
server.UseAuth<Auth0AuthProvider>(c => c.UseEmailPassword().UseGoogle().UseApple());
```

**Supabase**

```csharp
server.UseAuth<SupabaseAuthProvider>(c => c.UseEmailPassword().UseGoogle().UseGithub());
```

Before making any changes to your Program.cs, Ivy checks whether an authentication provider is already configured.
- If you selected a **different provider** than what is already configured, Ivy will ask you to confirm before overwriting the existing configuration.
- If you selected the **same provider**, Ivy reuses the existing configuration as defaults for your new setup.

> **Note:** Only one authentication provider can be active at a time. However, some providers (such as Auth0 or Supabase) support multiple login methods (like Google, GitHub, or username/password), so you can still offer users a variety of login options.

### Security and Secrets Management

Ivy automatically configures [.NET user secrets](../../02_Concepts/14_Secrets.md) for secure authentication configuration. To view configured secrets:

```terminal
>dotnet user-secrets list
```

#### Environment Variables

Instead of .NET user secrets, you can also use environment variables to store authentication secrets. For example, you might configure Auth0 like so:

**Windows (PowerShell):**

```terminal
>$env:Auth0__Domain="your-domain.auth0.com"
>$env:Auth0__ClientId="your-client-id"
>$env:Auth0__ClientSecret="your-client-secret"
>$env:Auth0__Audience="https://your-domain.auth0.com/api/v2"
>$env:Auth0__Namespace="https://ivy.app/"
```

**Mac/Linux (Bash):**
```terminal
>export Auth0__Domain="your-domain.auth0.com"
>export Auth0__ClientId="your-client-id"
>export Auth0__ClientSecret="your-client-secret"
>export Auth0__Audience="https://your-domain.auth0.com/api/v2"
>export Auth0__Namespace="https://ivy.app/"
```

If configuration is present in both .NET user secrets and environment variables, Ivy will use the values in [user secrets](../../02_Concepts/14_Secrets.md).

## Authentication Flow

### OAuth2 Flow (Auth0, Supabase, Microsoft Entra)

1. User visits your application.
2. User clicks "Login" and is redirected to the identity provider.
3. User authenticates with the identity provider (e.g., Google, Microsoft, GitHub).
4. The identity provider redirects back to your application's callback URL with an authorization code.
5. Ivy exchanges the authorization code for an access token, and in some cases a refresh token.
6. Ivy uses the token(s) to establish an authenticated session for the user.

### Email/Password Flow (Basic Auth, Authelia)

1. User visits your application.
2. User is prompted for a username and password.
3. Credentials are validated by the configured authentication provider.
4. If valid, Ivy establishes an authenticated session for the user.

## Using IAuthService in Views

Use [UseService](../../02_Concepts/01_Program.md) to obtain `IAuthService` in your [views](../../02_Concepts/02_Views.md):

```csharp
var auth = UseService<IAuthService>();

await auth.LoginAsync(email, password);
var user = await auth.GetUserInfoAsync();
await auth.LogoutAsync();
```

## Brokered Sessions

When users authenticate via OAuth providers (Google, GitHub, etc.) through your identity provider, the provider may store the OAuth provider's access tokens. Brokered sessions allow you to retrieve these tokens to call external APIs directly—for example, accessing Google Drive or calling the GitHub API using the user's credentials.

### Retrieving Brokered Sessions

Use `GetBrokeredSessionsAsync()` to retrieve brokered OAuth tokens:

```csharp
var authService = UseService<IAuthService>();
var result = await authService.GetBrokeredSessionsAsync();

if (result.Sessions?.TryGetValue(OAuthProviders.Google, out var googleSession) == true)
{
    // Use the Google OAuth token to call Google APIs
    using var httpClient = new HttpClient();
    httpClient.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", googleSession.AuthToken?.AccessToken);

    var response = await httpClient.GetAsync("https://www.googleapis.com/drive/v3/files");
    // Process response...
}
```

### Available OAuth Providers

The `OAuthProviders` class provides constants for supported providers:

- `OAuthProviders.Google` - Google
- `OAuthProviders.GitHub` - GitHub
- `OAuthProviders.Microsoft` - Microsoft
- `OAuthProviders.Apple` - Apple
- `OAuthProviders.Twitter` - Twitter
- `OAuthProviders.Discord` - Discord
- `OAuthProviders.Twitch` - Twitch
- `OAuthProviders.Figma` - Figma
- `OAuthProviders.Notion` - Notion
- `OAuthProviders.GitLab` - GitLab
- `OAuthProviders.Bitbucket` - Bitbucket

### Registering Token Handlers

For an OAuth provider to appear in the brokered sessions dictionary, there must be a registered `IAuthTokenHandler` for that provider. Token handlers manage token refresh, validation, and user info retrieval for brokered tokens.

**Built-in Handlers**

Ivy provides pre-built token handlers for Google and GitHub. Simply add the NuGet package to your project:

- `Ivy.Auth.Google` - Provides `GoogleAuthTokenHandler`
- `Ivy.Auth.GitHub` - Provides `GitHubAuthTokenHandler`

**Configuration for Google Token Handler**

The Google token handler requires your Google OAuth credentials to refresh tokens. Add these to your configuration (via [user secrets](../../02_Concepts/14_Secrets.md) or environment variables):

- **Google:ClientId**: Required. Your Google OAuth client ID.
- **Google:ClientSecret**: Required. Your Google OAuth client secret.

These are the same credentials you configured when setting up Google as a social login provider in your identity provider (Auth0, Clerk, Supabase, etc.).

**Custom Handlers**

For other OAuth providers, implement your own `IAuthTokenHandler` and register it:

```csharp
public class MyTwitterTokenHandler : IAuthTokenHandler
{
    public Task<AuthToken?> RefreshAccessTokenAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        // Implement token refresh logic
    }

    public Task<bool> ValidateAccessTokenAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        // Implement token validation logic
    }

    public Task<UserInfo?> GetUserInfoAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        // Implement user info retrieval
    }

    public Task<TokenLifetime?> GetAccessTokenLifetimeAsync(IAuthTokenHandlerSession authSession, CancellationToken cancellationToken = default)
    {
        // Implement token lifetime retrieval
    }
}
```

Register your handler in `Program.cs`:

```csharp
server.RegisterAuthTokenHandler<MyTwitterTokenHandler>(OAuthProviders.Twitter);
```

> **Note:** Without a registered token handler, the provider's tokens will not appear in `GetBrokeredSessionsAsync()` results even if the identity provider has them.

### Provider Support

Not all authentication providers support brokered sessions in the same way:

| Provider | Brokered Sessions Support |
|----------|--------------------------|
| Auth0 | Requires Management API access (see [Auth0 docs](02_Auth0.md)) |
| Clerk | Supported via Backend API |
| Supabase | Tokens captured at OAuth callback only |

See each provider's documentation for specific configuration requirements.

## Supported Authentication Providers

Ivy supports the following authentication providers. Click on any provider for detailed setup instructions:

- **[Auth0](02_Auth0.md)** - Universal authentication with social logins and enterprise integrations
- **[Clerk](02_Clerk.md)** - Modern authentication platform with passwordless login, social connections, and comprehensive user management
- **[Supabase](02_Supabase.md)** - Email/password, magic links, social auth, and Row Level Security integration
- **[Microsoft Entra](02_MicrosoftEntra.md)** - Enterprise SSO, conditional access, and Microsoft Graph integration
- **[Authelia](02_Authelia.md)** - Self-hosted identity provider with LDAP and forward auth
- **[Sliplane](02_Sliplane.md)** - OAuth 2.0 sign-in for apps deployed or integrated with Sliplane
- **[Basic Auth](02_BasicAuth.md)** - Simple username/password authentication for development and internal tools

## Examples

**Auth0 Setup**

```terminal
>ivy auth add --provider Auth0 --connection-string YourConnectionString
```

**Clerk Setup**

```terminal
>ivy auth add --provider Clerk --connection-string YourConnectionString
```

**Supabase Auth Setup**

```terminal
>ivy auth add --provider Supabase --connection-string YourConnectionString
```

**Basic Auth Setup**

```terminal
>ivy auth add --provider Basic --connection-string YourConnectionString
```

## Complete Custom Login View

For complete control over the login experience, you can replace the entire login view:

```csharp
server.UseAuth<BasicAuthProvider>(viewFactory: () => new MyCustomLoginApp());
```

## Customizing Authentication Cookies

Ivy allows you to customize authentication cookie settings globally from your [Program.cs](../../02_Concepts/01_Program.md) using the `Server.ConfigureAuthCookieOptions` static property. This enables you to override default cookie settings (such as expiration time, SameSite policy, Secure flag, etc.) based on your application's specific security requirements.

### Default Cookie Settings

By default, Ivy authentication cookies are configured with:
- **HttpOnly**: `true` (prevents JavaScript access)
- **Secure**: `true` (requires HTTPS — see [HTTPS in Development](#https-in-development) below)
- **SameSite**: `Strict` or `Lax` depending on the cookie (provides CSRF protection while allowing OAuth redirects)
- **Expires**: 1 year from creation
- **Path**: `/` (available site-wide)

### Customizing Cookie Options

To override these defaults, set `Server.ConfigureAuthCookieOptions` in your `Program.cs` before calling `server.RunAsync()`:

```csharp
Server.ConfigureAuthCookieOptions = options =>
{
    options.Expires = DateTimeOffset.UtcNow.AddDays(30);
};
```

> **Note**: Custom configuration is applied after Ivy sets the default values, allowing you to override any setting. It's recommended to keep `HttpOnly = true` for security.

### Namespacing Cookies for Multiple Apps

When running multiple Ivy auth apps on the same domain (e.g., on different ports or subpaths), they would normally share cookies by domain, causing login conflicts. The `Server.AuthCookiePrefix` property solves this by namespacing cookies per app, ensuring that each app's authentication cookies remain isolated.

**Configuration Example**:

```csharp
Server.AuthCookiePrefix = "clerk";
```

**Naming Scheme**: The prefix uses a double-underscore delimiter to avoid ambiguity with existing brokered session cookies (which use single underscore, e.g., `google_access_token`):

| Cookie (no prefix) | Cookie (prefix = `"clerk"`) | Cookie (prefix = `"basic"`) |
|---|---|---|
| `access_token` | `clerk__access_token` | `basic__access_token` |
| `refresh_token` | `clerk__refresh_token` | `basic__refresh_token` |
| `auth_tag` | `clerk__auth_tag` | `basic__auth_tag` |
| `auth_session_data` | `clerk__auth_session_data` | `basic__auth_session_data` |
| `google_access_token` (brokered) | `clerk__google_access_token` | `basic__google_access_token` |

**Brokered Session Isolation**: Brokered OAuth session cookies (e.g., for Google, GitHub) are also prefixed, maintaining isolation between apps even when using the same OAuth providers.

**Typical Use Cases**: Container apps that embed multiple auth examples as iframes, or development scenarios where multiple Ivy apps run on `localhost` with different ports.

## HTTPS in Development

Ivy uses secure (`Secure = true`) authentication cookies, which means your browser will only send them over HTTPS. In local development, Ivy automatically serves over HTTPS using the ASP.NET Core development certificate.

### macOS and Windows

Run the following command once to generate and trust the development certificate:

```terminal
>dotnet dev-certs https --trust
```

After this, your app will be accessible at `https://localhost:5010`.

### Linux

Linux requires additional manual steps to trust the development certificate — see [Installation](../../01_GettingStarted/02_Installation.md#prerequisites) for details.

## Best Practices

**Security** - Always use HTTPS in production, store sensitive configuration in user secrets or environment variables, regularly rotate client secrets, use strong passwords for Basic Auth, and implement proper session management.

**Configuration** - Use descriptive names for your authentication providers, keep configuration separate from code, use environment-specific settings, and document your authentication setup.

**Testing** - Test authentication flows in development, verify token validation works correctly, and ensure logout functionality works properly.

## Troubleshooting

**Authentication Provider Issues** - Verify your provider configuration is correct, check that your project is properly registered with the identity provider, ensure callback URLs are correctly configured, and verify network connectivity to the authentication provider.

**Token Validation Issues** - Check that your JWT tokens are properly signed, verify audience and issuer claims, and ensure your system clock is set correctly.

**Configuration Issues** - Ensure authentication settings are properly stored in [user secrets](../../02_Concepts/14_Secrets.md) (or verify environment variables are correctly set), and check that your [Program.cs](../../02_Concepts/01_Program.md) includes the necessary authentication config.

### Related Commands

- `ivy init` - Initialize a new Ivy project
- `ivy db add` - Add database connections
- `ivy app create` - Create apps
- `ivy deploy` - Deploy your project
