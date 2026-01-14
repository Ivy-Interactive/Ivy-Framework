---
searchHints:
  - clerk
  - authentication
  - oauth
  - social-login
  - identity
  - auth
---

# Clerk Authentication Provider

<Ingress>
Secure your Ivy application with Clerk's modern authentication platform featuring social logins, passwordless authentication, and advanced security features.
</Ingress>

## Overview

Clerk is a complete authentication and user management platform designed for modern web applications. It provides a seamless authentication experience with support for multiple identity providers, passwordless login, multi-factor authentication, and comprehensive user management.

Clerk offers both **development** and **production** modes, allowing you to test authentication flows locally before deploying to production.

## Getting Your Clerk Configuration

Before using Clerk with Ivy, you'll need to create a Clerk application and obtain your API keys.

### Step 1: Create a Clerk Account and Application

1. **Sign up** at [Clerk](https://clerk.com) if you don't have an account
2. **Go to** the [Clerk Dashboard](https://dashboard.clerk.com)
3. **Click "Create application"**
4. **Enter an application name** (e.g., "My Ivy App")
5. **Choose your sign in options** (you can change these later)
6. **Click "Create application"**

### Step 2: Get Your API Keys

To find your API keys, go to **Configure → API keys** in your Clerk Dashboard. You'll need to copy two keys:

- **Publishable key**: Starts with `pk_test_` (development) or `pk_live_` (production)
- **Secret key**: Starts with `sk_test_` (development) or `sk_live_` (production)

![Clerk API Keys](/ivy/assets/clerk_api_keys.webp "Clerk API Keys")

> **Important**: Keep your secret key secure. Never commit it to version control or expose it in client-side code.

### Step 3: Configure Allowed Origins

For development, Clerk automatically allows `localhost` origins. For production:

1. **Go to "Configure → Developers → Domains"** in your Clerk Dashboard
2. **Add your production domain** (e.g., `https://myapp.com`)
3. **Click "Add Domain"**
4. **Follow the instructions** to add the required CNAME records to your domain

### Step 4: Enable Authentication Methods

Clerk supports multiple authentication methods. Configure the ones you want to use:

#### Email and Password

Email and password authentication is enabled by default. To configure:

1. **Go to "Configure → User & authentication"** in your Clerk Dashboard
2. **Click "Email"**
3. **Enable or disable "Sign-in with email"** as needed
4. **Click "Save"**

#### Username and Password

To enable username and password authentication:

1. **Go to "Configure → User & authentication"** in your Clerk Dashboard
2. **Click "Username"**
3. **Enable "Sign-in with username"**
4. **Click "Save"**

#### Social Connections (OAuth)

Clerk supports numerous social authentication providers:

1. **Go to "Configure → User & authentication -> SSO connections"** in your Clerk Dashboard
2. **Click "Add connection", then "For all users"**
3. **Select a provider you wish to enable** (Google, GitHub, Microsoft, Apple, etc.)
4. **Configure OAuth credentials** for your provider if using custom settings, as required in production
5. **Configure other settings for your provider**
6. **Click "Apply"**

> **Note**: Clerk provides development OAuth credentials for testing. For production, you'll need to configure your own OAuth applications with each provider. See [Clerk's OAuth documentation](https://clerk.com/docs/authentication/social-connections/overview) for detailed setup instructions.

### Step 5: Configure Session Token

For Ivy to access user information from Clerk, you need to add custom claims to your session tokens:

1. **Go to "Configure → Sessions"** in your Clerk Dashboard
2. **Click "Edit" in the "Customize session token" section**
3. **Add the following JSON configuration:**

```json
{
    "email": "{{user.primary_email_address}}",
    "username": "{{user.username}}",
    "full_name": "{{user.full_name}}",
    "image_url": "{{user.image_url}}",
    "has_image": "{{user.has_image}}"
}
```

4. **Click "Save"**

These custom claims allow Ivy to extract user information (email, username, full name, and profile image) from the JWT token for use in your application.

### Development vs Production Modes

Clerk provides separate API keys for development and production:

- **Development Keys** (`pk_test_*` and `sk_test_*`):
  - Used for local development and testing
  - Include built-in OAuth credentials for social providers
  - Allow `localhost` origins by default
  - Data is separate from production

- **Production Keys** (`pk_live_*` and `sk_live_*`):
  - Used for deployed applications
  - Require custom OAuth credentials for social providers
  - Must explicitly configure allowed domains and add required CNAME records
  - Separate user database from development

#### Creating a Production Instance

When you first create a Clerk application, it starts in development mode. To deploy your application to production, you'll need to create a production instance:

1. **Go to your application** in the Clerk Dashboard
2. **Click the application name dropdown** at the top of the dashboard
3. **Click "Create production instance"**
4. **Choose "Clone development instance" and click "Continue"**
5. **Choose "Clone development instance" and click "Create Instance"**
6. **Setup social connection credentials** for all services you wish to support. For production instances, you must configure custom credentials for each service.
7. **Connect domains** by configuring CNAME records for your production domain
   - Set up OAuth providers with production credentials (if using social logins)
8. **Get your production API keys** from **Configure → API keys** after creation
9. **Update your deployed application** with the new `pk_live_*` and `sk_live_*` keys

Your development and production instances are completely separate, with different:

- User databases
- Authentication settings
- API keys
- Configured domains

This separation allows you to safely test changes in development without affecting production users.

## Adding Authentication

To set up Clerk Authentication with Ivy, run the following command and choose `Clerk` when asked to select an auth provider:

```terminal
>ivy auth add
```

You will be prompted to provide the following Clerk configuration:

- **Publishable Key**: Your Clerk publishable key (starts with `pk_test_` or `pk_live_`)
- **Secret Key**: Your Clerk secret key (starts with `sk_test_` or `sk_live_`)

Your credentials will be stored securely in .NET user secrets.

Ivy then finishes configuring your application automatically:

1. Adds the `Ivy.Auth.Clerk` package to your project
2. Adds `server.UseAuth<ClerkAuthProvider>()` to your `Program.cs`
3. Adds `Ivy.Auth.Clerk` to your global usings

### Advanced Configuration

#### Connection Strings

To skip the interactive prompts, you can provide configuration via a connection string:

```terminal
>ivy auth add --provider Clerk --connection-string "Clerk:PublishableKey=pk_test_xxxxx; Clerk:SecretKey=sk_test_xxxxx;"
```

For a list of connection string parameters, see [Configuration Parameters](#configuration-parameters) below.

#### Manual Configuration

When deploying an Ivy project without using `ivy deploy`, your local .NET user secrets are not automatically transferred. In that case, you can configure Clerk auth by setting environment variables or .NET user secrets. See Configuration Parameters below.

> **Note:** If configuration is present in both .NET user secrets and environment variables, Ivy will use the values in **.NET user secrets over environment variables**.

For more information, see [Authentication Overview](01_AuthenticationOverview.md).

#### Configuration Parameters

The following parameters are supported via connection string, environment variables, or .NET user secrets:

- **Clerk:SecretKey**: Required. Your Clerk secret key (starts with `sk_test_` or `sk_live_`).
- **Clerk:PublishableKey**: Required. Your Clerk publishable key (starts with `pk_test_` or `pk_live_`).

## Authentication Flow

Clerk uses a modern authentication flow that works seamlessly with Ivy.

### Session-based Flow

1. User visits your Ivy application
2. Clerk checks for an existing session
3. If no session exists, user is prompted to authenticate
4. User chooses an authentication method (email/password, social login, etc.)
5. Clerk handles the authentication and creates a session
6. Session token is securely stored and used for subsequent requests
7. Ivy validates the session token on the backend
8. Ivy automatically refreshes the session token as needed

## Clerk-Specific Features

Key features of the Clerk provider:

- **User Management**: Comprehensive dashboard for managing users, sessions, and authentication settings
- **Session Management**: Automatic token refresh and secure session handling
- **Customizable UI**: Pre-built authentication components that can be customized to match your brand
- **Development Mode**: Separate test environment with built-in OAuth credentials for easy local development

## Security Best Practices

- **Always use HTTPS** in production environments
- **Use production keys** (`sk_live_*` and `pk_live_*`) only in deployed environments
- **Never commit secret keys** to version control
- **Rotate API keys** if they are ever exposed
- **Configure domains** properly to prevent unauthorized access
- **Monitor authentication logs** in the Clerk Dashboard
- **Use environment-appropriate keys** for testing and production

## Troubleshooting

### Common Issues

**Invalid API Keys**

- Verify your Secret Key and Publishable Key are correct
- Ensure both keys are from the same environment (both test or both live)
- Check that keys haven't been rotated in the Clerk Dashboard
- If using connection strings, ensure the format is correct

**Domain Configuration Issues**

- Verify allowed domains are configured in Clerk Dashboard
- Check that your application's domain matches exactly (including https://)
- Verify no typos in domain configuration

**Authentication Failed**

- Check that your Clerk application is properly configured
- Verify required authentication methods are enabled in Clerk Dashboard
- Ensure users are not blocked or suspended

**Session Issues**

- Verify session tokens are being properly stored in cookies
- Check that cookies are not being blocked by browser settings
- Ensure your application's domain matches the configured origins
- Verify HTTPS is enabled in production

**Development vs Production Key Mismatch**

- If you see "must both be for the same environment" error, you're mixing test and live keys
- Use `sk_test_*` with `pk_test_*` for development
- Use `sk_live_*` with `pk_live_*` for production
- Never mix keys from different environments

**Social Login Not Working**

- In development: Built-in OAuth credentials should work automatically
- In production: Ensure you've configured custom OAuth credentials for each provider
- Verify OAuth redirect URLs are properly configured
- Check provider-specific settings in Clerk Dashboard

## Related Documentation

- [Authentication Overview](01_AuthenticationOverview.md)
- [Auth0 Authentication](02_Auth0.md)
- [Supabase Authentication](02_Supabase.md)
- [Clerk Official Documentation](https://clerk.com/docs)
- [Clerk Social Connections](https://clerk.com/docs/authentication/social-connections/overview)
