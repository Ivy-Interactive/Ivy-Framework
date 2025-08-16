# Ivy Authentication

<Ingress>
Secure your Ivy application with integrated authentication providers including Auth0, Supabase, Authelia, and Microsoft Entra ID.
</Ingress>

The `ivy auth` commands allow you to add and configure authentication providers in your Ivy project. Ivy supports several authentication providers and helps you to integrate them with your project.

## Supported Authentication Providers

Ivy supports the following authentication providers:

### Identity Providers

- **Auth0** - Universal authentication platform
- **Supabase Auth** - Built-in authentication for Supabase
- **Microsoft Entra** - Identity and access management from Microsoft
- **Authelia** - Open-source identity provider

### Basic Authentication

- **Basic Auth** - Simple username/password authentication

### Basic Usage

### Adding an Authentication Provider

```terminal
>ivy auth add
```

This command will:

- Prompt you to select an authentication provider
- Prompt you for necessary configuration details
- Update your `Program.cs` with the necessary authentication setup
- Store sensitive configuration in .NET User Secrets

### Command Options

`--provider <PROVIDER>` - Specify the authentication provider directly:

```terminal
>ivy auth add --provider Auth0
```

Available providers: `Auth0`, `Supabase`, `MicrosoftEntra`, `Authelia`, `Basic`

`--connection-string <CONNECTION_STRING>` - Provide provider-specific configuration using connection string syntax:

```terminal
>ivy auth add --provider Auth0 --connection-string YourConnectionString
```

`--verbose` - Enable verbose output for detailed logging:

```terminal
>ivy auth add --verbose
```

### Interactive Mode

When you run `ivy auth add` without specifying options, Ivy will guide you through an interactive setup:

1. **Select Authentication Provider**: Choose from the available providers
2. **Provider Configuration**: Enter the necessary configuration details
3. **Integration Setup**: Ivy will automatically configure your project

### Authentication Provider Configuration

**Auth0** - Universal authentication platform that supports multiple identity providers.

**Setup Process**

```terminal
>ivy auth add --provider Auth0
```

**Required Configuration** - Domain (e.g., `your-project.auth0.com`), Client ID, and Client Secret.

**Connection String Format**

```text
Domain=your-domain.auth0.com;ClientId=your-client-id;ClientSecret=your-client-secret
```

**Auth0 Application Setup** - Create an application in your Auth0 dashboard, set the callback URL to `https://your-project.com/callback`, configure allowed logout URLs, and note your Domain, Client ID, and Client Secret.

**Supabase Auth** - Built-in authentication for Supabase projects.

**Setup Process**

```terminal
>ivy auth add --provider Supabase
```

**Required Configuration** - Project URL, Anon Key, and optionally Service Role Key for admin operations.

**Connection String Format**

```text
ProjectUrl=https://your-project.supabase.co;AnonKey=your-anon-key;ServiceRoleKey=your-service-role-key
```

**Supabase Project Setup** - Create a project in Supabase, go to Settings > API, copy your Project URL and anon key, and optionally copy your service role key for admin operations.

**Authelia** - Open-source identity provider that can integrate with various authentication systems.

**Setup Process**

```terminal
>ivy auth add --provider Authelia
```

**Required Configuration** - Base URL, Client ID, and Client Secret.

**Connection String Format**

```text
BaseUrl=https://auth.your-domain.com;ClientId=your-client-id;ClientSecret=your-client-secret
```

**Authelia Setup** - Install and configure Authelia, create an OAuth2 client for your application, configure the redirect URIs, and note your Base URL, Client ID, and Client Secret.

**Basic Auth** - Simple username/password authentication.

**Setup Process**

```terminal
>ivy auth add --provider Basic
```

**Required Configuration** - Username and Password.

**Connection String Format**

```text
Username=admin;Password=secure-password
```

### Security and Secrets Management

Ivy automatically configures .NET User Secrets for secure authentication configuration. To view configured secrets:

```terminal
>dotnet user-secrets list
```

### Environment Variables

You can also use environment variables for authentication configuration:

```text
export AUTH0_DOMAIN="your-domain.auth0.com"
export AUTH0_CLIENT_ID="your-client-id"
export AUTH0_CLIENT_SECRET="your-client-secret"
export AUTH0_AUDIENCE="https://your-domain.auth0.com/api/v2"
```

## Program.cs Integration

Ivy automatically parses and updates your `Program.cs` to configure authentication:

**Basic Auth**

```csharp
server.UseAuth<BasicAuthProvider>();
```

**Auth0 Integration**

```csharp
server.UseAuth<Auth0AuthProvider>(c => c.UseEmailPassword().UseGoogle().UseApple());
```

**Supabase Auth Integration**

```csharp
server.UseAuth<SupabaseAuthProvider>(c => c.UseEmailPassword().UseGoogle().UseApple());
```

**Microsoft Entra Integration**

```csharp
server.UseAuth<MicrosoftEntraAuthProvider>(c => c.UseMicrosoftEntra());
```

## Authentication Flow

### OAuth2 Flow (Auth0, Supabase, Microsoft Entra)

1. User visits your application
2. User clicks "Login" and is redirected to the identity provider
3. User authenticates with the identity provider
4. User is redirected back to your application with an authorization code
5. Your application exchanges the code for an access token
6. The access token is used to authenticate API requests

### Email/Password Flow (Basic Auth, Authelia)

1. User visits your application
2. Browser prompts for username/password
3. Credentials are validated
4. If valid, user is authenticated for the session

## Troubleshooting

**Authentication Provider Issues** - Verify your provider configuration is correct, check that your project is properly registered with the identity provider, ensure callback URLs are correctly configured, and verify network connectivity to the authentication provider.

**Token Validation Issues** - Check that your JWT tokens are properly signed, verify audience and issuer claims, and ensure your project's clock is synchronized.

**Configuration Issues** - Ensure authentication settings are properly stored in user secrets, verify environment variables are correctly set, and check that your `Program.cs` includes the necessary authentication middleware.

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

### Related Commands

- `ivy init` - Initialize a new Ivy project
- `ivy db add` - Add database connections
- `ivy app create` - Create apps
- `ivy deploy` - Deploy your project