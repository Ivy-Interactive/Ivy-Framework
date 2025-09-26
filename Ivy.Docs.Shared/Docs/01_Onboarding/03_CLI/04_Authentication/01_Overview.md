# Ivy Authentication

<Ingress>
Secure your Ivy application with integrated authentication providers including Auth0, Supabase, Authelia, and Microsoft Entra ID.
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
3. **Project Setup**: Ivy updates your `Program.cs` and stores sensitive values in .NET user secrets, so your project is ready to use authentication.

### Command Options

`--provider <PROVIDER>` or `-p <PROVIDER>` - Specify the authentication provider directly:

```terminal
>ivy auth add --provider Auth0
```

Available providers: `Auth0`, `Supabase`, `MicrosoftEntra`, `Authelia`, `Basic`

`--connection-string <CONNECTION_STRING>` - Provide provider-specific configuration using connection string syntax. When used with `--provider`, this option allows you pass all required configuration inline, rather than being prompted interactively during setup:

```terminal
>ivy auth add --provider Auth0 --connection-string YourConnectionString
```

`--verbose` or `-v` - Enable verbose output for detailed logging:

```terminal
>ivy auth add --verbose
```

### How Ivy Updates Program.cs

Ivy automatically updates your `Program.cs` to configure authentication. Here are a few examples of code that it may add:

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

Before making any changes to your `Program.cs`, Ivy checks whether an authentication provider is already configured.
- If you selected a **different provider** than what is already configured, Ivy will ask you to confirm before overwriting the existing configuration.
- If you selected the **same provider**, Ivy reuses the existing configuration as defaults for your new setup.

> **Note:** Only one authentication provider can be active at a time. However, some providers (such as Auth0 or Supabase) support multiple login methods (like Google, GitHub, or username/password), so you can still offer users a variety of login options.

### Security and Secrets Management

Ivy automatically configures .NET user secrets for secure authentication configuration. To view configured secrets:

```terminal
>dotnet user-secrets list
```

#### Environment Variables

Instead of .NET user secrets, you can also use environment variables to store authentication secrets. For example, you might configure Auth0 like so:

**Windows (PowerShell):**

```powershell
$env:AUTH0_DOMAIN="your-domain.auth0.com"
$env:AUTH0_CLIENT_ID="your-client-id"
$env:AUTH0_CLIENT_SECRET="your-client-secret"
$env:AUTH0_AUDIENCE="https://your-domain.auth0.com/api/v2"
```

**Mac/Linux (Bash):**
```bash
export AUTH0_DOMAIN="your-domain.auth0.com"
export AUTH0_CLIENT_ID="your-client-id"
export AUTH0_CLIENT_SECRET="your-client-secret"
export AUTH0_AUDIENCE="https://your-domain.auth0.com/api/v2"
```

If configuration is present in both .NET user secrets and environment variables, Ivy will use the values in .NET user secrets.

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

## Supported Authentication Providers

Ivy supports the following authentication providers. Click on any provider for detailed setup instructions:

- **[Auth0](Auth0.md)** - Universal authentication with social logins and enterprise integrations
- **[Supabase](Supabase.md)** - Email/password, magic links, social auth, and Row Level Security integration
- **[Microsoft Entra](MicrosoftEntra.md)** - Enterprise SSO, conditional access, and Microsoft Graph integration
- **[Authelia](Authelia.md)** - Self-hosted identity provider with LDAP and forward auth
- **[Basic Auth](BasicAuth.md)** - Simple username/password authentication for development and internal tools

## Examples

**Auth0 Setup**

```terminal
>ivy auth add --provider Auth0 --connection-string YourConnectionString
```

**Supabase Auth Setup**

```terminal
>ivy auth add --provider Supabase --connection-string YourConnectionString
```

**Basic Auth Setup**

```terminal
>ivy auth add --provider Basic --connection-string YourConnectionString
```

### Best Practices

**Security** - Always use HTTPS in production, store sensitive configuration in user secrets or environment variables, regularly rotate client secrets, use strong passwords for Basic Auth, and implement proper session management.

**Configuration** - Use descriptive names for your authentication providers, keep configuration separate from code, use environment-specific settings, and document your authentication setup.

**Testing** - Test authentication flows in development, verify token validation works correctly, and ensure logout functionality works properly.

## Troubleshooting

**Authentication Provider Issues** - Verify your provider configuration is correct, check that your project is properly registered with the identity provider, ensure callback URLs are correctly configured, and verify network connectivity to the authentication provider.

**Token Validation Issues** - Check that your JWT tokens are properly signed, verify audience and issuer claims, and ensure your system clock is set correctly.

**Configuration Issues** - Ensure authentication settings are properly stored in user secrets (or verify environment variables are correctly set), and check that your `Program.cs` includes the necessary authentication config.

### Related Commands

- `ivy init` - Initialize a new Ivy project
- `ivy db add` - Add database connections
- `ivy app create` - Create apps
- `ivy deploy` - Deploy your project
