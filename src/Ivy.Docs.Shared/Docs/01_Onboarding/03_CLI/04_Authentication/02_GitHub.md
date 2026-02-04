---
title: GitHub
searchHints:
  - github
  - authentication
  - oauth
  - social-login
  - identity
  - git
---
# GitHub Authentication Provider

<Ingress>
Secure your Ivy application with GitHub OAuth 2.0 authentication, enabling users to sign in using their GitHub accounts.
</Ingress>

## Overview

The GitHub authentication provider enables OAuth 2.0 authentication using GitHub accounts. This allows users to sign in to your Ivy application using their existing GitHub credentials, providing a seamless authentication experience for developers and technical users.

## Getting Your GitHub Configuration

Before using GitHub authentication with Ivy, you'll need to create a GitHub OAuth App and obtain your configuration values:

### Step 1: Create a GitHub OAuth App

1. **Go to** [GitHub Settings > Developer settings > OAuth Apps](https://github.com/settings/developers)
2. **Click "New OAuth App"**
3. Fill in the application details:
   - **Application name**: Your application name (e.g., "My Ivy Application")
   - **Homepage URL**: Your application URL (e.g., `http://localhost:5010` for local development or `https://your-app.com` for production)
   - **Authorization callback URL**: Your application's webhook endpoint (e.g., `http://localhost:5010/ivy/webhook` for local development or `https://your-app.com/ivy/webhook` for production)
4. **Click "Register application"**

> **Note**: The Authorization callback URL must match your application's webhook endpoint. For local development, this is typically `http://localhost:PORT/ivy/webhook` where PORT is your application's port.

### Step 2: Get Your Configuration Values

After creating your OAuth App, you'll see the application's settings page. Copy these values:

- **Client ID**: Found in the "Client ID" field
- **Client Secret**: Click "Generate a new client secret" to create one, then copy it immediately (you won't be able to see it again)
- **Redirect Uri**: The authorization callback URL you configured during OAuth App creation (e.g., `http://localhost:5010/ivy/webhook` for local development or `https://your-domain.com/ivy/webhook` for production)

> **Important**: Store your Client Secret securely. The Client Secret is sensitive and won't be retrievable from GitHub after creation - if you lose it, you'll need to generate a new one. Use [.NET user secrets](../../02_Concepts/14_Secrets.md) for local development and environment variables or secure key management for production. Never commit secrets to version control.

## Adding Authentication

To set up GitHub Authentication with Ivy, you need to manually configure it in your project:

### Manual Configuration

**1: Register the HttpClient factory in your [Program.cs](../../02_Concepts/01_Program.md)**:

```csharp
var server = new Server();

server.Services.AddHttpClient("GitHubAuth", client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "Ivy-Framework");
    client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
    client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
});

// Ensure IConfiguration is registered
server.Services.AddSingleton(server.Configuration);
```

**2: Configure the GitHub Auth Provider**:

```csharp
using Ivy.Auth.GitHub;

// Configure GitHub Auth Provider - UseAuth will create the provider via DI
server.UseAuth<GitHubAuthProvider>(c => c.UseGitHub());

await server.RunAsync();
```

**3: Add configuration via [.NET user secrets](../../02_Concepts/14_Secrets.md) or environment variables. See [Configuration Parameters](#configuration-parameters) below for detailed instructions.**

### Configuration Parameters

Configure the following required parameters using .NET user secrets (recommended for development) or environment variables (recommended for production):

- **GitHub:ClientId**: Required. Your GitHub OAuth App's Client ID.
- **GitHub:ClientSecret**: Required. Your GitHub OAuth App's Client Secret.
- **GitHub:RedirectUri**: Required. The authorization callback URL that matches your GitHub OAuth App settings.

**Using .NET User Secrets (Development):**

```terminal
>dotnet user-secrets set "GitHub:ClientId" "your_client_id"
>dotnet user-secrets set "GitHub:ClientSecret" "your_client_secret"
>dotnet user-secrets set "GitHub:RedirectUri" "your_redirect_uri"
```

**Using Environment Variables (Production):**

```terminal
$env:GitHub__ClientId="your_client_id"
$env:GitHub__ClientSecret="your_client_secret"
$env:GitHub__RedirectUri="your_redirect_uri"
```

> **Note:** If configuration is present in both .NET user secrets and environment variables, Ivy will use the values in **.NET user secrets over environment variables**. Never commit secrets to version control.

## Authentication Flow

### OAuth Flow

1. User clicks the "GitHub" login button in your Ivy application
2. User is redirected to GitHub's authorization page
3. User authorizes your application to access their GitHub account
4. GitHub redirects back to your application's webhook endpoint (`/ivy/webhook`)
5. Ivy receives the authorization code and exchanges it for an access token
6. User is authenticated and redirected back to your application
7. User can now access your Ivy application with their GitHub identity

## GitHub-Specific Features

Key features of the GitHub authentication provider:

- **OAuth 2.0 Flow**: Standard OAuth 2.0 authorization code flow
- **User Information**: Automatically retrieves user ID, email, display name, and avatar
- **Email Handling**: Automatically selects the primary email address or first verified email
- **Long-lived Tokens**: GitHub OAuth tokens are long-lived and don't expire
- **No Refresh Tokens**: GitHub doesn't provide refresh tokens, but tokens are long-lived

### Required Scopes

The GitHub OAuth app requests the following scopes:

- `user:email` - Access to user's email addresses (required for user identification)

## Security Best Practices

- **Always use HTTPS** in production environments
- **Store Client Secret securely** - use .NET user secrets for development and environment variables or secure key management for production
- **Never commit secrets** to version control
- **Rotate Client Secret** if it's compromised
- **Use appropriate callback URLs** - ensure the callback URL in your GitHub OAuth App matches your application's webhook endpoint exactly
- **Monitor authentication logs** in your application for suspicious activity

## Troubleshooting

### Common Issues

**Invalid Client Credentials**

- Verify Client ID and Client Secret are correct in your configuration
- Check that credentials haven't been regenerated in GitHub OAuth App settings
- Ensure there are no extra spaces or characters in the configuration values

**Callback URL Mismatch**

- Verify Authorization callback URL in GitHub OAuth App settings matches your application's webhook endpoint (e.g., `http://localhost:5010/ivy/webhook`)
- Check that the URL matches exactly, including the protocol (`http://` vs `https://`) and port number
- Ensure HTTPS is used in production

**Authentication Failed**

- Check that your GitHub OAuth App is properly configured
- Verify the OAuth App is not suspended or deleted
- Ensure users have authorized the application (they may need to revoke and re-authorize)

**Email Not Available**

- Verify the user's GitHub account has a verified email address
- Check that the `user:email` scope is properly requested (this is automatic)
- Some GitHub accounts may not have public email addresses

**Token Exchange Errors**

- Verify your application is accessible at the configured callback URL
- Check network connectivity and firewall settings
- Ensure your application is running and the webhook endpoint is accessible

## Related Documentation

- [Authentication Overview](01_AuthenticationOverview.md)
- [Auth0 Authentication](02_Auth0.md)
- [Basic Auth](02_BasicAuth.md)
- [Supabase Authentication](02_Supabase.md)
