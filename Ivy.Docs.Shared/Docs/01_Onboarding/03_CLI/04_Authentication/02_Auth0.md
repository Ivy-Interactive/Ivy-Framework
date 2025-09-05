# Auth0 Authentication Provider

<Ingress>
Secure your Ivy application with Auth0's universal authentication platform supporting multiple identity providers and social logins.
</Ingress>

## Overview

Auth0 is a universal authentication and authorization platform that provides secure authentication for applications. It supports multiple identity providers, social logins, and enterprise integrations.

## Getting Your Auth0 Configuration

Before using Auth0 with Ivy, you'll need to create an Auth0 application and obtain your configuration values:

### Step 1: Create an Auth0 Account and Application

1. **Sign up** at [Auth0](https://auth0.com) if you don't have an account
2. **Go to** the [Auth0 Dashboard](https://manage.auth0.com/dashboard) and navigate to **Applications**: ![Auth0 Applications Option in Sidebar](assets/auth0_applications.webp "Auth0 Applications Option in Sidebar")
3. **Click "Create Application"**
4. **Choose "Regular Web Applications"** as the application type
5. **Click "Create"**: ![Auth0 Create Application Panel](assets/auth0_create_application.webp "Auth0 Create Application Panel")

### Step 2: Configure Your Application

In your application settings:

1. **Set Allowed Callback URLs** to `http://localhost:5010/webhook`, replacing the base URL (`http://localhost:5010`) with your application's URL: ![Auth0 Allowed Callback URLs](assets/auth0_callback.webp "Auth0 Allowed Callback URLs")
4. **Click "Save"**

### Step 3: Get Your Configuration Values

In your application settings, copy these values from the Basic Information section:

- **Domain**
- **Client ID**
- **Client Secret**

![Auth0 Basic Information](assets/auth0_basic_information.webp "Auth0 Basic Information")

### Step 4: Create an API

1. **Go to APIs** in the Auth0 Dashboard
2. **Click "Create API"**
3. **Enter a name** (e.g., "My API")
4. **Set an identifier** (e.g., `https://my-application.com/api`)
5. **Click "Create"**
6. **Copy the Identifier** - this is your **Audience** value

![Auth0 Create API](assets/auth0_create_api.webp "Auth0 Create API")

## Adding Authentication

To set up Auth0 Authentication with Ivy, run the following command and choose `Auth0` when asked to select an auth provider:

```terminal
>ivy auth add
```

You will be prompted to provide the following Auth0 configuration:

- **Auth0 Domain**: Your Auth0 tenant domain (e.g., `your-project.auth0.com`)
- **Client ID**: Your Auth0 application's client ID
- **Client Secret**: Your Auth0 application's client secret
- **Audience**: API identifier for securing API access

Your credentials will be stored securely in .NET user secrets. Ivy then finishes configuring your application automatically:

1. Adds the `Ivy.Auth.Auth0` package to your project.
2. Adds `server.UseAuth<Auth0AuthProvider>(c => c.UseEmailPassword().UseGoogle().UseApple());` to your `Program.cs`.
3. Adds `Ivy.Auth.Auth0` to your global usings.

### Connection String Format

To skip the interactive prompts, you can provide configuration via a connection string parameter:

```terminal
>ivy auth add --provider Auth0 --connection-string "AUTH0_DOMAIN=your-domain.auth0.com;AUTH0_CLIENT_ID=your-client-id;AUTH0_CLIENT_SECRET=your-client-secret"
```

The connection string uses the following parameters:

- **AUTH0_DOMAIN**: Required. Your Auth0 tenant domain.
- **AUTH0_CLIENT_ID**: Required. Your Auth0 application's client ID.
- **AUTH0_CLIENT_SECRET**: Required. Your Auth0 application's client secret.
- **AUTH0_AUDIENCE**: Required. API identifier for securing API access.

### Advanced Configuration

The following parameters can be manually set via .NET user secrets or environment variables:

- **AUTH0_DOMAIN**: Your Auth0 tenant domain. Set by `ivy auth add`.
- **AUTH0_CLIENT_ID**: Your Auth0 application's client ID. Set by `ivy auth add`.
- **AUTH0_CLIENT_SECRET**: Your Auth0 application's client secret. Set by `ivy auth add`.
- **AUTH0_AUDIENCE**: API identifier for API authentication. Set by `ivy auth add`.

If configuration is present in both .NET user secrets and environment variables, Ivy will use the values in .NET user secrets. For more information, see [Authentication Overview](Overview.md).

## Authentication Flow

### Email/Password Flow
1. User enters email and password directly in your Ivy application
2. Ivy sends credentials to Auth0 for validation
3. Auth0 validates credentials and returns access tokens
4. User is authenticated and can access protected resources

### Social Login Flow
1. User clicks a social login button in your application
2. User is redirected to the social provider (Google, Apple, etc.)
3. User authenticates with the social provider
4. Provider redirects back to Auth0, then to your application
5. Ivy receives and processes the authentication tokens

## Auth0-Specific Features

Key features of the Auth0 provider:

- **Dual Authentication Modes**: Both direct email/password forms and social login redirects
- **Social Connections**: Support for Google, Apple, GitHub, Twitter, and Microsoft
- **Enterprise SSO**: SAML, Active Directory, and other enterprise integrations
- **User Management**: Built-in user management dashboard and APIs

## Security Best Practices

- **Always use HTTPS** in production environments
- **Store secrets securely** in user secrets or environment variables
- **Configure proper scopes** and audience claims for API access
- **Monitor authentication logs** in Auth0 Dashboard
- **Rotate client secrets** periodically

## Troubleshooting

### Common Issues

**Invalid Client Credentials**
- Verify Client ID and Client Secret are correct and match your Auth0 application
- Check that credentials haven't been regenerated in Auth0 Dashboard
- Ensure connection string format is correct

**Callback URL Mismatch**
- Verify Allowed Callback URLs in Auth0 Dashboard include your application's callback URL
- Check that the callback URL matches your application URL exactly
- Ensure HTTPS is used in production

**Authentication Failed**
- Check that your Auth0 application is properly configured
- Verify social connections are enabled and configured in Auth0 Dashboard
- Ensure users exist and are not blocked in Auth0 Dashboard

**Token Issues**
- Verify audience and issuer claims if using API authentication
- Check clock synchronization between servers
- Ensure refresh tokens are working properly for seamless session management

## Related Documentation

- [Authentication Overview](Overview.md)
- [Supabase Authentication](Supabase.md)
- [Microsoft Entra Provider](MicrosoftEntra.md)