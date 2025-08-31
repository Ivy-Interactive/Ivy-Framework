# Microsoft Entra Authentication Provider

<Ingress>
Secure your Ivy application with Microsoft Entra ID for enterprise identity and access management with SSO and conditional access.
</Ingress>

## Overview

Microsoft Entra ID (formerly Azure Active Directory) is Microsoft's cloud-based identity and access management service. It provides secure authentication for enterprise applications, supports single sign-on (SSO), multi-factor authentication, and integrates seamlessly with Microsoft 365 and Azure services.

## Getting Your Microsoft Entra Configuration

Before using Microsoft Entra with Ivy, you'll need to register an application and obtain your configuration values:

### Step 1: Register Your Application

1. **Sign in** to the [Microsoft Entra admin center](https://entra.microsoft.com)
2. **Navigate** to **Identity > Applications > App registrations**
3. **Click "New registration"**
4. **Fill in the details**:
   - **Name**: Your application name (e.g., "My Ivy App")
   - **Supported account types**: Choose based on your needs:
     - **Single tenant**: Only your organization
     - **Multitenant**: Any organization
     - **Multitenant + personal**: Any organization + personal Microsoft accounts
   - **Redirect URI**: Select "Web" and enter `http://localhost:5010/webhook` (replace with your app's URL)
5. **Click "Register"**

### Step 2: Get Your Configuration Values

From your app registration **Overview** page, copy these values:

- **Application (client) ID**: This is your **Client ID**
- **Directory (tenant) ID**: This is your **Tenant ID**

### Step 3: Create a Client Secret

1. **Go to "Certificates & secrets"** in your app registration
2. **Click "New client secret"**
3. **Add a description** (e.g., "Ivy App Secret")
4. **Choose an expiration** (6 months, 12 months, or 24 months)
5. **Click "Add"**
6. **Copy the secret Value** immediately (this is your **Client Secret**)
   - ⚠️ **Important**: The secret value is only shown once. Copy it now!

### Step 4: Configure Authentication

1. **Go to "Authentication"** in your app registration
2. **Under "Redirect URIs"**, ensure your callback URL is listed
3. **Under "Implicit grant and hybrid flows"**, check **"ID tokens"**
4. **Click "Save"**

### Step 5: Set API Permissions (Optional)

If you need to access Microsoft Graph or other APIs:

1. **Go to "API permissions"**
2. **Click "Add a permission"**
3. **Choose "Microsoft Graph"**
4. **Select "Delegated permissions"**
5. **Add permissions** like `User.Read`, `Mail.Read`, etc.
6. **Click "Add permissions"**
7. **Grant admin consent** if required (click "Grant admin consent")

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

### Connection String Format

To skip the interactive prompts, you can provide configuration via a connection string parameter:

```terminal
>ivy auth add --provider MicrosoftEntra --connection-string "MICROSOFT_ENTRA_TENANT_ID=your-tenant-id;MICROSOFT_ENTRA_CLIENT_ID=your-client-id;MICROSOFT_ENTRA_CLIENT_SECRET=your-client-secret"
```

The connection string uses the following parameters:

- **MICROSOFT_ENTRA_TENANT_ID**: Required. Your Microsoft Entra tenant ID.
- **MICROSOFT_ENTRA_CLIENT_ID**: Required. Your application's client ID.
- **MICROSOFT_ENTRA_CLIENT_SECRET**: Required. Your application's client secret.

### Advanced Configuration

The following parameters can be manually set via .NET user secrets or environment variables:

- **MICROSOFT_ENTRA_TENANT_ID**: Your Microsoft Entra tenant ID. Set by `ivy auth add`.
- **MICROSOFT_ENTRA_CLIENT_ID**: Your application's client ID. Set by `ivy auth add`.
- **MICROSOFT_ENTRA_CLIENT_SECRET**: Your application's client secret. Set by `ivy auth add`.

If configuration is present in both .NET user secrets and environment variables, Ivy will use the values in .NET user secrets. For more information, see [Authentication Overview](Overview.md).

## Authentication Flow

1. User clicks a login button in your application
2. User is redirected to Microsoft Entra with appropriate parameters
3. User authenticates with their Microsoft credentials
4. Microsoft Entra redirects back to your application with authorization code
5. Ivy exchanges the authorization code for access and ID tokens
6. The user is authenticated with their Microsoft identity

## Microsoft Entra-Specific Features

Key features of the Microsoft Entra provider:

- **Enterprise SSO**: Single sign-on with existing Microsoft 365 and Azure accounts
- **Conditional Access**: Fine-grained access policies based on user, location, device, and risk
- **Multi-Factor Authentication**: Built-in MFA support through conditional access policies
- **Microsoft Graph Integration**: Access to Microsoft 365 data and services
- **Azure Integration**: Seamless integration with Azure services and managed identities

## Security Best Practices

- **Always use HTTPS** in production environments
- **Store secrets securely** in Azure Key Vault or user secrets
- **Enable MFA** through conditional access policies
- **Configure appropriate token lifetimes** for your security requirements
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
- Verify audience and issuer claims match your configuration
- Check that system clocks are synchronized
- Ensure refresh tokens are working properly for seamless session management

## Related Documentation

- [Authentication Overview](Overview.md)
- [Auth0 Provider](Auth0.md)
- [Supabase Provider](Supabase.md)