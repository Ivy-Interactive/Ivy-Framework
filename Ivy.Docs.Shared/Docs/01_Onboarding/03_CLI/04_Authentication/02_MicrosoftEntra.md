# Microsoft Entra Authentication Provider

<Ingress>
Secure your Ivy application with Microsoft Entra ID for enterprise identity and access management with SSO and conditional access.
</Ingress>

## Overview

Microsoft Entra ID (formerly Azure Active Directory) is Microsoft's cloud-based identity and access management service. It provides secure authentication for enterprise applications, supports single sign-on (SSO), and integrates seamlessly with Microsoft 365 and Azure services.

## Getting Your Microsoft Entra Configuration

Before using Microsoft Entra with Ivy, you'll need to register an application and obtain your configuration values:

### Step 1: Register Your Application

1. **Sign in** to the [Microsoft Entra admin center](https://entra.microsoft.com)
2. **Navigate** to **Entra ID > App registrations** in the sidebar
3. **Click "New registration"**
4. **Fill in the details**:
   - **Name**: Your application name (e.g., "My Ivy App")
   - **Supported account types**: Choose based on your needs:
     - **Accounts in this organizational directory only (Single tenant)**: Only users in your organization can sign in.
     - **Accounts in any organizational directory (Multitenant)**: Users in any organization's Entra ID can sign in.
     - **Accounts in any organizational directory (Multitenant) and personal Microsoft accounts**: Both organizational accounts and consumer Microsoft accounts (e.g., Skype, Xbox) can sign in.
     - **Personal Microsoft accounts only**: Only consumer Microsoft accounts can sign in.
   - **Redirect URI**: Select "Web" and enter `http://localhost:5010/webhook`. Replace the base URL (`http://localhost:5010`) with your application's URL.
5. **Click "Register"**

### Step 2: Get Your Configuration Values

From your app registration **Overview** page, copy these values:

- **Application (client) ID**: This is your **Client ID**
- **Directory (tenant) ID**: This is your **Tenant ID**

![Entra client and tenant IDs](assets/entra_client_and_tenant_ids.webp "Entra client and tenant IDs")

### Step 3: Create a Client Secret

1. **Go to "Certificates & secrets"** in your app registration
2. **Click "New client secret"**
3. **Add a description** (e.g., "Ivy App Secret")
4. **Choose an expiration** (e.g., 6 months, 12 months, or 24 months)
5. **Click "Add"**
6. **Copy the secret Value** (this is your **Client Secret**). Note that only **Value** is needed, not **Secret ID**.

![Newly-created Entra client secret](assets/entra_client_secret.webp "Newly-created Entra client secret")

> ⚠️ **Warning:** This secret value will only be shown once, so copy it before leaving the page.

### Step 4: Configure Authentication

1. **Go to "Authentication"** in your app registration
2. **Under "Redirect URIs"**, ensure your callback URL is listed
3. **Under "Implicit grant and hybrid flows"**, check **"ID tokens"**
4. **Click "Save"**

![ID tokens enabled](assets/entra_id_tokens_enabled.webp "ID tokens enabled")

### Step 5: Set API Permissions

First, check for the `User.Read` permission, required by Ivy:
1. **Go to "API permissions"** in your app registration
2. **Verify that `User.Read` is shown and enabled under "Microsoft Graph"**

If `User.Read` is not enabled:
1. **Click "Add a permission"**
2. **Choose "Microsoft Graph"**
3. **Select "Delegated permissions"**
4. **Search for and enable `User.Read`**
5. **Click "Add permissions"**

## Adding Authentication

To set up Microsoft Entra Authentication with Ivy, run the following command and choose `MicrosoftEntra` when asked to select an auth provider:

```terminal
>ivy auth add
```

You will be prompted to provide the following Microsoft Entra configuration:

- **Tenant ID**: Your Microsoft Entra tenant ID
- **Client ID**: Your application's client ID (Application ID)
- **Client Secret**: Your application's client secret

Your credentials will be stored securely in .NET user secrets. Ivy then finishes configuring your application automatically:

1. Adds the `Ivy.Auth.MicrosoftEntra` package to your project.
2. Adds `server.UseAuth<MicrosoftEntraAuthProvider>(c => c.UseMicrosoftEntra());` to your `Program.cs`.
3. Adds `Ivy.Auth.MicrosoftEntra` to your global usings.

### Advanced Configuration

#### Connection Strings

To skip the interactive prompts, you can provide configuration via a connection string:

```terminal
>ivy auth add --provider MicrosoftEntra --connection-string "MICROSOFT_ENTRA_TENANT_ID=your-tenant-id;MICROSOFT_ENTRA_CLIENT_ID=your-client-id;MICROSOFT_ENTRA_CLIENT_SECRET=your-client-secret"
```

For a list of connection string parameters, see [Configuration Parameters](#configuration-parameters) below.

#### Manual Configuration

When deploying an Ivy project without using `ivy deploy`, your local .NET user secrets are not automatically transferred. In that case, you can configure Entra Auth by setting environment variables or .NET user secrets. See [Configuration Parameters](#configuration-parameters) below.

> **Note:** If configuration is present in both .NET user secrets and environment variables, Ivy will use the values in **.NET user secrets over environment variables**.

For more information, see [Authentication Overview](Overview.md).

#### Configuration Parameters

The following parameters are supported via connection string, environment variables, or .NET user secrets:

- **MICROSOFT_ENTRA_TENANT_ID**: Required. Your Microsoft Entra tenant ID.
- **MICROSOFT_ENTRA_CLIENT_ID**: Required. Your application's client ID.
- **MICROSOFT_ENTRA_CLIENT_SECRET**: Required. Your application's client secret.

## Authentication Flow

1. User clicks a login button in your application
2. User is redirected to Microsoft Entra with appropriate parameters
3. User authenticates with their Microsoft credentials
4. Microsoft Entra redirects back to your application with authorization code
5. Ivy exchanges the authorization code for access and ID tokens
6. The user is authenticated with their Microsoft identity

## Microsoft Entra-Specific Features

- **Enterprise Authentication**: OAuth2-based authentication with Microsoft accounts
- **Microsoft Identity Integration**: Uses Microsoft's identity platform for user authentication

## Security Best Practices

- **Always use HTTPS** in production environments
- **Monitor sign-in logs** in Microsoft Entra admin center
- **Use managed identities** when running on Azure
- **Implement proper logout** to clear sessions

## Troubleshooting

### Common Issues

**Invalid Client Credentials**
- Verify Client ID and Client Secret are correct and match your Microsoft Entra application
- Check that the client secret hasn't expired (secrets have expiration dates)
- Ensure you're using the secret value, not the secret ID

**Redirect URI Mismatch**
- Verify redirect URIs in Microsoft Entra match your application URLs exactly
- Check for case sensitivity in URLs
- Ensure HTTPS is used in production environments

**Authentication Failed**
- Check that your Microsoft Entra application is properly configured
- Verify users exist in your tenant and are not blocked
- Ensure the application has appropriate permissions granted

**Token Issues**
- Ensure refresh tokens are working properly for seamless session management

## Related Documentation

- [Authentication Overview](Overview.md)
- [Auth0 Provider](Auth0.md)
- [Supabase Provider](Supabase.md)
- [Microsoft Entra ID Documentation](https://learn.microsoft.com/en-us/entra/identity/)
- [Register an application in Microsoft Entra ID](https://learn.microsoft.com/en-us/entra/identity-platform/quickstart-register-app)
- [Microsoft identity platform app types and authentication flows](https://learn.microsoft.com/en-us/entra/identity-platform/authentication-flows-app-scenarios)