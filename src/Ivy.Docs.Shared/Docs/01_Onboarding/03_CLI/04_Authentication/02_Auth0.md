---
searchHints:
  - auth0
  - authentication
  - oauth
  - social-login
  - identity
  - sso
---

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
2. **Go to** the [Auth0 Dashboard](https://manage.auth0.com/dashboard) and navigate to **Applications**: ![Auth0 Applications Option in Sidebar](/ivy/ivy-assets/auth0_applications.webp "Auth0 Applications Option in Sidebar")
3. **Click "Create Application"**
4. **Choose "Regular Web Applications"** as the application type
5. **Click "Create"**: ![Auth0 Create Application Panel](/ivy/ivy-assets/auth0_create_application.webp "Auth0 Create Application Panel")

### Step 2: Configure Your Application

1. Navigate to your application's **Settings** tab: ![Auth0 Settings Tab](/ivy/ivy-assets/auth0_application_settings_tab.webp "Auth0 Settings Tab")
2. **Set Allowed Callback URLs** to `http://localhost:5010/ivy/auth/callback`, replacing the base URL (`http://localhost:5010`) with your application's URL: ![Auth0 Allowed Callback URLs](/ivy/ivy-assets/auth0_callback.webp "Auth0 Allowed Callback URLs")
3. **Click "Save"**

### Step 3: Get Your Configuration Values

In your application settings, copy these values from the Basic Information section:

- **Domain**
- **Client ID**
- **Client Secret**

![Auth0 Basic Information](/ivy/ivy-assets/auth0_basic_information.webp "Auth0 Basic Information")

### Step 4: Create an API

1. **Go to APIs** in the Auth0 Dashboard
2. **Click "Create API"**
3. **Enter a name** (e.g., "My API")
4. **Set an identifier** (e.g., `https://my-application.com/api`)
5. **Set "JSON Web Token (JWT) Profile" to "Auth0" and "JSON Web Token (JWT) Signing Algorithm" to "RS256"**
6. **Click "Create"**
7. **Copy the Identifier** - this is your **Audience** value

![Auth0 Create API](/ivy/ivy-assets/auth0_create_api.webp "Auth0 Create API")

### Step 5: Enable Authentication Options

You'll need to enable the specific authentication options you want to use in your Auth0 tenant:

#### Email and Password

1. **Go to Authentication > Database** in the Auth0 Dashboard: ![Auth0 Database](/ivy/ivy-assets/auth0_database.webp "Auth0 Database")
2. **Click on the "Username-Password-Authentication" connection**: ![Auth0 Database Connections](/ivy/ivy-assets/auth0_database_connections.webp "Auth0 Database Connections")
3. **Go to the "Applications" tab and ensure that the connection is enabled on your application**: ![Auth0 Connection Enabled](/ivy/ivy-assets/auth0_connection_enabled.webp "Auth0 Connection Enabled")
4. **Go to the "Settings" tab and enable "Disable Sign Ups"** if you want to control user registration manually
5. **Go to the "Authentication Methods" tab and configure your password policy** as needed for your security requirements
6. **Enable the Password Grant**: In your Auth0 application, go to the **Settings** tab, scroll down to **Advanced Settings**, click **Grant Types**, and ensure the **Password** grant type is checked: ![Auth0 Password Grant](/ivy/ivy-assets/auth0_password_grant.webp "Auth0 Password Grant")

##### Adding Users for Email and Password Authentication

Once you've enabled the "Username-Password-Authentication" connection, you'll need to add users. The simplest way to do this is in the Auth0 Dashboard:

1. **Go to User Management > Users** in the Auth0 Dashboard
2. **Click "Create User"**, then click **"Create via UI"** in the dropdown that appears
3. **Select the "Username-Password-Authentication" connection**
4. **Enter the user's email address**
5. **Set the user's password**
6. **Click "Create"**

For more information about user creation and management, see Auth0's [Manage Users documentation](https://auth0.com/docs/manage-users).

#### Google

First, go to Authentication > Social in the Auth0 Dashboard. If "google-oauth2" is already listed, configure it:

1. **Enter your Google Client ID and Client Secret** (from Google Cloud Console)
2. **Enable "Offline Access"** if you want to allow users to remain logged in for an extended period of time
3. **Enable the connection** for your application in the "Applications" tab

Otherwise, create and configure the connection:

1. **Click "Create Connection"**
2. **Select "Google / Gmail" from the list of social providers**
3. **Click "Continue"**
4. **Enter your Google Client ID and Client Secret** (from Google Cloud Console)
5. **Enable "Offline Access"** if you want to allow users to remain logged in for an extended period of time
6. **Enable the connection** for your application in the "Applications" tab

> **Note**: For testing Google authentication, you can use [Auth0 Development Keys](https://auth0.com/docs/authenticate/identity-providers/social-identity-providers/devkeys) by leaving Client ID and Client Secret blank. These keys are not to be used for production.

For more information on setting up Google authentication, see Auth0's [Google documentation](https://marketplace.auth0.com/integrations/google-social-connection).

#### GitHub

1. **Go to Authentication > Social** in the Auth0 Dashboard
2. **Click "Create Connection"**
3. **Select "GitHub" from the list of social providers**
4. **Click "Continue"**
5. **Enter your GitHub Client ID and Secret** (from GitHub Developer Settings)
6. **Configure scopes** ("Email address" is recommended)
7. **Click "Create"**
8. **Enable the connection** for your application in the "Applications" tab

> **Note**: For testing GitHub authentication, you can use Auth0 Development Keys by leaving Client ID and Client Secret blank. These keys are not to be used for production.

For more information on setting up GitHub authentication, see Auth0's [GitHub documentation](https://marketplace.auth0.com/integrations/github-social-connection).

#### Microsoft

1. **Go to Authentication > Social** in the Auth0 Dashboard
2. **Click "Create Connection"**
3. **Select "Microsoft Account" from the list of social providers**
4. **Click "Continue"**
5. **Enter your Microsoft Application ID and Secret** (from Azure Portal)
6. **Enable "Offline Access"** if you want to allow users to remain logged in for an extended period of time
7. **Click "Create"**
8. **Enable the connection** for your application in the "Applications" tab

> **Note**: Auth0 Development Keys can not be used for Microsoft authentication.

For more information on setting up Microsoft authentication, see Auth0's [Microsoft documentation](https://marketplace.auth0.com/integrations/microsoft-account-social-connection).

#### Apple

1. **Go to Authentication > Social** in the Auth0 Dashboard
2. **Click "Create Connection"**
3. **Select "Apple" from the list of social providers**
4. **Click "Continue"**
5. **Enter your Client ID, Client Secret Signing Key, Apple Team ID, and Key ID** (from Apple Developer Portal)
6. **Configure allowed scopes** (email, name are recommended)
7. **Click "Create"**
8. **Enable the connection** for your application in the "Applications" tab

> **Note**: For testing Apple authentication, you can use Auth0 Development Keys by leaving Client ID, Client Secret Signing Key, Apple Team ID, and Key ID blank. These development keys are not to be used for production.

For more information on setting up Apple authentication, see Auth0's [Apple documentation](https://marketplace.auth0.com/integrations/apple-social-connection).

#### Twitter/X

1. **Go to Authentication > Social** in the Auth0 Dashboard
2. **Click "Create Connection"**
3. **Select "Twitter" from the list of social providers**
4. **Click "Continue"**
5. **Enter your Twitter API Key and Secret** (from Twitter Developer Portal)
6. **Click "Create"**
7. **Enable the connection** for your application in the "Applications" tab

> **Note**: For testing Twitter/X authentication, you can use Auth0 Development Keys by leaving API Key and Secret blank. These development keys are not to be used for production.

For more information on setting up Twitter/X authentication, see Auth0's [Twitter documentation](https://marketplace.auth0.com/integrations/twitter-social-connection).

### Step 6: Add claims to JWT

To allow the Ivy application to properly display the users email, name and avatar, these need to be added to the JWT. In Auth0, this is done using an action.

1. **Go to Actions > Library** in the Auth0 Dashboard
2. **Click "Create Action" > "Create Custom Action"**
3. **Give it a name like "Add user claims to JWT" and choose "Login / Post Login" as the trigger.**
4. **Add the following code:**

```javascript
exports.onExecutePostLogin = async (event, api) => {
    // You can change this if you want - but then also update the "Auth0:Namespace" configuration in your Ivy project
    let namespace = "https://ivy.app/";

    if (namespace && !namespace.endsWith('/')) {
        namespace += '/';
    }

    api.accessToken.setCustomClaim(namespace + 'email', event.user.email);
    api.accessToken.setCustomClaim(namespace + 'name', event.user.name);
    api.accessToken.setCustomClaim(namespace + 'avatar', event.user.picture);
};
```

5. **Click Deploy**
6. **Go to Actions > Triggers** in the Auth0 Dashboard
7. **Click "post-login"**
8. **Add your action** by dragging it from the right-hand panel between "Start" and "Complete".

The user information email, name and picture will now be added to the JWT and consumed by Ivy in the `GetUserInfoAsync` function.

### Step 7: Enable Management API Access (Optional)

If you need to access brokered sessions (e.g., to call Google APIs directly using the Google OAuth token), you must authorize your application to access the Auth0 Management API. For more information about brokered sessions, see [Authentication Overview](01_AuthenticationOverview.md#brokered-sessions).

#### Why You Need This

When users authenticate via OAuth providers like Google or GitHub, Auth0 manages the OAuth flow and stores the provider's access tokens. By default, your application cannot access these tokens. Enabling Management API access allows you to retrieve these tokens programmatically.

#### Configuration Steps

1. **Go to Applications > APIs** in the Auth0 Dashboard
2. **Click on "Auth0 Management API"**
3. **Select the "Application Access" tab**
4. **Find your application** in the list of applications
5. **Click "Edit"**
6. **Select the "Client Access" tab if it isn't already selected**
7. **Toggle the switch to "Authorized"**
8. **Grant the following scopes**:
   - `read:users` - Required to fetch user information
   - `read:user_idp_tokens` - Required to access OAuth provider tokens
9. **Click "Update"**

#### Register Token Handlers

In addition to enabling Management API access, you must have a registered token handler for each OAuth provider you want to use. Without a registered handler, the provider won't appear in brokered sessions.

**Built-in handlers** - Add the NuGet package to your project:
- `Ivy.Auth.Google` - For Google OAuth tokens (requires `Google:ClientId` and `Google:ClientSecret` configuration)
- `Ivy.Auth.GitHub` - For GitHub OAuth tokens

> **Note:** The Google token handler requires your Google OAuth credentials (`Google:ClientId` and `Google:ClientSecret`) to refresh tokens. These are the same credentials you configured in Auth0 for Google social login.

**Custom handlers** - For other providers (Microsoft, Apple, Twitter), implement a custom `IAuthTokenHandler` and register it in `Program.cs`:

```csharp
server.RegisterAuthTokenHandler<MyTwitterTokenHandler>(OAuthProviders.Twitter);
```

See [Authentication Overview](01_AuthenticationOverview.md#registering-token-handlers) for details on implementing custom token handlers.

#### Using Brokered Sessions

Once configured, you can access OAuth provider tokens in your Ivy application:

```csharp
var authService = UseService<IAuthService>();

// Get all brokered sessions
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

**Available OAuth Providers:**
- `OAuthProviders.Google` - Google
- `OAuthProviders.GitHub` - GitHub
- `OAuthProviders.Twitter` - Twitter
- `OAuthProviders.Apple` - Apple
- `OAuthProviders.Microsoft` - Microsoft

> **Note:** If Management API access is not configured, `GetBrokeredSessionsAsync()` will throw an exception.

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

Your credentials will be stored securely in [.NET user secrets](../../02_Concepts/14_Secrets.md). You will then be prompted to choose one or more authentication options to support, from the following list:

- **E-mail and password**
- **Google**
- **GitHub**
- **Microsoft**
- **Apple**
- **Twitter**

> **Note**: All options selected must also be enabled in your Auth0 Dashboard to work.

Ivy then finishes configuring your application automatically:

1. Adds the `Ivy.Auth.Auth0` package to your project.
2. Dynamically generates and adds the appropriate `UseAuth<Auth0AuthProvider>()` call to your [Program.cs](../../02_Concepts/01_Program.md) based on your selected options (e.g., if you select Email/Password and Google, it generates: `server.UseAuth<Auth0AuthProvider>(c => c.UseEmailPassword().UseGoogle());`).
3. Adds `Ivy.Auth.Auth0` to your global usings.

### Advanced Configuration

#### Connection Strings

To skip the interactive prompts, you can provide configuration via a connection string:

```terminal
>ivy auth add --provider Auth0 --connection-string "Auth0:Domain=your-domain.auth0.com;Auth0:ClientId=your-client-id;Auth0:ClientSecret=your-client-secret;Auth0:Namespace=https://your-domain.com/"
```

For a list of connection string parameters, see [Configuration Parameters](#configuration-parameters) below.

#### Manual Configuration

When deploying an Ivy project without using `ivy deploy`, your local [.NET user secrets](../../02_Concepts/14_Secrets.md) are not automatically transferred. In that case, you can configure basic auth by setting environment variables or .NET user secrets. See Configuration Parameters below.

> **Note:** If configuration is present in both .NET user secrets and environment variables, Ivy will use the values in **[.NET user secrets](../../02_Concepts/14_Secrets.md) over environment variables**.

For more information, see [Authentication Overview](01_AuthenticationOverview.md).

#### Configuration Parameters

The following parameters are supported via connection string, environment variables, or .NET user secrets:

- **Auth0:Domain**: Required. Your Auth0 tenant domain.
- **Auth0:ClientId**: Required. Your Auth0 application's client ID.
- **Auth0:ClientSecret**: Required. Your Auth0 application's client secret.
- **Auth0:Audience**: Required. API identifier for securing API access.
- **Auth0:Namespace**: Optional. Your JWT claims namespace, as chosen earlier in this guide. If left empty "<https://ivy.app/>" will be used.

## Authentication Flow

### Email/Password Flow

1. User enters email and password directly in your Ivy application
2. Ivy sends credentials to Auth0 for validation
3. Auth0 validates credentials and returns access tokens
4. User is authenticated and can access your Ivy application

### Social Login Flow

1. User clicks a social login button in your application
2. User is redirected to the social provider (Google, Apple, etc.)
3. User authenticates with the social provider
4. Provider redirects back to Auth0, then to your application
5. Ivy receives and processes the authentication tokens

## Auth0-Specific Features

Key features of the Auth0 provider:

- **Two Authentication Modes**: Both direct email/password forms and social login redirects
- **Social Connections**: Support for Google, Apple, GitHub, Twitter, and Microsoft
- **User Management**: Built-in user management dashboard and APIs

## Security Best Practices

- **Always use HTTPS** in production environments
- **Configure proper scopes** and audience claims for API access
- **Monitor authentication logs** in Auth0 Dashboard
- **Rotate client secrets** periodically

## Troubleshooting

### Common Issues

**Invalid Client Credentials**

- Verify Client ID, Client Secret and other required values are correct in your Auth0 configuration
- Check that credentials haven't been regenerated in Auth0 Dashboard
- If using connection strings, ensure the format is correct

**Callback URL Mismatch**

- Verify Allowed Callback URLs in Auth0 Dashboard include your application's callback URL
- Check that the callback URL matches your application URL exactly
- Ensure HTTPS is used in production

**Authentication Failed**

- Check that your Auth0 application is properly configured
- Verify social connections are enabled and configured in Auth0 Dashboard
- Ensure users exist and are not blocked in Auth0 Dashboard

**Token Issues**

- Verify audience and issuer claims
- Ensure refresh tokens are working properly for seamless session management

## Related Documentation

- [Authentication Overview](01_AuthenticationOverview.md)
- [Supabase Authentication](02_Supabase.md)
- [Microsoft Entra Provider](02_MicrosoftEntra.md)
