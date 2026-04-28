---
name: ivy-create-auth-connection
description: >
  Add an authentication provider to an Ivy project. Supports Basic Auth (JWT),
  Auth0, Supabase, Clerk, GitHub OAuth, Authelia, and Microsoft Entra ID.
  Use when the user wants to add login, authentication, or identity to their project.
allowed-tools: Bash(dotnet:*) Bash(ivy:*) Read Write Edit Glob Grep
effort: medium
argument-hint: "[auth provider name, e.g. Auth0, Supabase, Clerk]"
---

# ivy-create-auth-connection

Add an authentication provider to an existing Ivy project. This skill guides you through selecting a provider, collecting the required credentials, and configuring the project.

## Step 1: Validate the Project

1. Verify this is a valid Ivy project. Check for a `.csproj` file and `Program.cs` in the working directory. If this is not an Ivy project, tell the user and stop.

## Step 2: Choose an Auth Provider

2. If the user has not already specified a provider, ask them to choose one from the supported list:

| Provider | Description | Required Secrets |
|---|---|---|
| **Basic** | Simple username/password auth with JWT tokens. Good for internal tools or prototyping. | Users, HashSecret, JwtSecret, JwtIssuer, JwtAudience |
| **Auth0** | Enterprise-grade identity platform with social logins, MFA, and SSO. | Domain, ClientId, ClientSecret, Audience, Namespace |
| **Supabase** | Open-source Firebase alternative with built-in auth. Supports email/password, social logins, and SSO via WorkOS. | Url, ApiKey, LegacyJwtSecret |
| **Clerk** | Modern authentication with prebuilt UI components and session management. | PublishableKey, SecretKey |
| **GitHub** | GitHub OAuth for developer-facing applications. | ClientId, ClientSecret, RedirectUri |
| **Authelia** | Self-hosted single sign-on and 2FA server. | Url |
| **Microsoft Entra** | Azure Active Directory / Microsoft Entra ID for enterprise SSO. | TenantId, ClientId, ClientSecret |

## Step 3: Collect Provider-Specific Credentials

3. Based on the chosen provider, collect the required credentials from the user. Each provider requires different configuration values:

### Basic Auth (JWT)
- Ask the user for initial user credentials (username:password pairs, comma-separated)
- Ask for a JWT issuer name (default: the project name)
- Ask for a JWT audience name (default: the project name)
- A hash secret and JWT secret are auto-generated

### Auth0
- Ask for the Auth0 Domain (e.g. `your-tenant.auth0.com`)
- Ask for the Client ID from the Auth0 application
- Ask for the Client Secret from the Auth0 application
- Ask for the API Audience (e.g. `https://your-api.example.com`)
- Optionally, Auth0 supports additional options like role-based access control and custom claims

### Supabase
- Ask for the Supabase project URL (e.g. `https://your-project.supabase.co`)
- Ask for the Supabase anon/public API key
- Optionally, ask for the legacy JWT secret (for direct JWT verification)
- Supabase supports additional options like WorkOS SSO integration

### Clerk
- Ask for the Clerk Publishable Key (starts with `pk_`)
- Ask for the Clerk Secret Key (starts with `sk_`)
- Clerk supports additional options like organization-based access

### GitHub OAuth
- Ask for the GitHub OAuth App Client ID
- Ask for the GitHub OAuth App Client Secret
- Ask for the Redirect URI (e.g. `https://localhost:5001/callback`)

### Authelia
- Ask for the Authelia instance URL (e.g. `https://auth.example.com`)

### Microsoft Entra ID
- Ask for the Tenant ID
- Ask for the Application (Client) ID
- Ask for the Client Secret

## Step 4: Generate the Auth Configuration

4. The auth provider setup involves:
   - Adding the required NuGet package for the provider
   - Storing secrets in dotnet user secrets (with the provider-specific prefix)
   - Updating `Program.cs` with `server.UseAuth<[ProviderClassName]>()`

5. Initialize user secrets for the project:

```bash
dotnet user-secrets init
```

6. Set each required secret using `dotnet user-secrets set`. For example:

```bash
dotnet user-secrets set "Auth0:Domain" "your-tenant.auth0.com"
dotnet user-secrets set "Auth0:ClientId" "your-client-id"
```

7. Use `ivy docs` or `ivy ask` to look up the specific auth provider documentation if you need details about the NuGet package name, the provider class name, or the `Program.cs` registration pattern for the chosen provider.

## Step 5: Verify

8. Run `dotnet build` to verify everything compiles. Fix any errors.

9. If the project is in a git repository, create a commit with a descriptive message, for example: "Added [ProviderDisplayName] authentication."

10. Tell the user the auth provider is ready and summarize what was configured:
    - Auth provider package added
    - Secrets stored with the provider prefix
    - Program.cs updated with the auth registration

## Recovery

If the setup fails:

1. Diagnose the root cause (invalid credentials, auth provider not configured, network issues):
   - For invalid credentials: verify API keys, tokens, or OAuth client credentials are correct
   - For provider config issues: check that the auth provider is registered in the app's configuration
   - For network issues: verify the auth provider endpoint is accessible

2. After fixing the underlying issue, either retry using the `/ivy-create-auth-connection` skill from scratch, or manually create the auth provider class and register it in `Program.cs`.

3. Use `ivy ask "How do I configure [ProviderName] authentication?"` for provider-specific guidance.
