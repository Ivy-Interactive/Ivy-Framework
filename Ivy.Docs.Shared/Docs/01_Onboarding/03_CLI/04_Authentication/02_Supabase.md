# Supabase Authentication Provider

<Ingress>
Secure your Ivy application with Supabase's built-in authentication supporting email/password and social logins.
</Ingress>

## Overview

Supabase Auth provides a complete authentication system built on top of PostgreSQL. It offers email/password authentication, social logins, and integrates seamlessly with Supabase's database and real-time features for a comprehensive backend-as-a-service solution.

## Getting Your Supabase Configuration

Before using Supabase with Ivy, you'll need to create a Supabase project and obtain your configuration values:

### Step 1: Create a Supabase Account and Project

1. **Sign up** at [Supabase](https://supabase.com) if you don't have an account
2. **Click "New Project"**
3. **Choose your organization** (or create one)
4. **Enter project details**:
   - **Name**: Your project name
   - **Database Password**: A secure password
   - **Region**: Choose closest to your users
5. **Click "Create new project"**
6. **Wait** for the project to be created (this can take a few minutes)

### Step 2: Get Your Configuration Values

Once your project is ready:

1. **Go to your project dashboard**
2. **Click on "Settings"** in the sidebar
3. **Click on "API"**
4. **Copy these values**:
   - **Project URL**: Found under "Project URL" (e.g., `https://your-project-id.supabase.co`)
   - **Anon Key**: Found under "Project API keys" → "anon public" (this is your API key)

### Step 3: Configure Authentication (Optional)

To customize your authentication settings:

1. **Go to Authentication** in the sidebar
2. **Click "Settings"**
3. **Configure**:
   - **Site URL**: Your application's URL (e.g., `http://localhost:5010`)
   - **Redirect URLs**: Add your callback URLs (e.g., `http://localhost:5010/webhook/*`)

### Step 4: Enable Social Providers (Optional)

If you want to use social login providers:

1. **Go to Authentication > Providers**
2. **Click on the provider** you want to enable (Google, GitHub, etc.)
3. **Toggle "Enable sign in with [Provider]"**
4. **Enter the required credentials** (Client ID, Client Secret from the provider)
5. **Save**

## Adding Authentication

To set up Supabase Authentication with Ivy, run the following command and choose `Supabase` when asked to select an auth provider:

```terminal
>ivy auth add
```

You will be prompted to provide the following Supabase configuration:

- **Project URL**: Your Supabase project URL (e.g., `https://your-project.supabase.co`)
- **API Key**: Your Supabase project's anonymous (anon) key

Your credentials will be stored securely in .NET user secrets. Ivy then finishes configuring your application automatically:

1. Adds the `Ivy.Auth.Supabase` package to your project.
2. Adds `server.UseAuth<SupabaseAuthProvider>(c => c.UseEmailPassword().UseGoogle().UseApple());` to your `Program.cs`.
3. Adds `Ivy.Auth.Supabase` to your global usings.

### Advanced Configuration

#### Connection Strings

To skip the interactive prompts, you can provide configuration via a connection string:

```terminal
>ivy auth add --provider Supabase --connection-string "SUPABASE_URL=https://your-project.supabase.co;SUPABASE_API_KEY=your-anon-key"
```

For a list of connection string parameters, see [Configuration Parameters](#configuration-parameters) below.

#### Manual Configuration

When deploying an Ivy project without using `ivy deploy`, your local .NET user secrets are not automatically transferred. In that case, you can configure basic auth by setting environment variables or .NET user secrets. See [Configuration Parameters](#configuration-parameters) below.

> **Note:** If configuration is present in both .NET user secrets and environment variables, Ivy will use the values in **.NET user secrets over environment variables**.

For more information, see [Authentication Overview](Overview.md).

#### Configuration Parameters

The following parameters are supported via connection string, environment variables, or .NET user secrets:

- **SUPABASE_URL**: Required. Your Supabase project URL.
- **SUPABASE_API_KEY**: Required. Your Supabase project's anonymous key.

## Authentication Flow

### Email/Password Flow

1. User provides credentials in your Ivy application
2. Supabase validates credentials and creates a session
3. User receives access token for authenticated requests
4. Ivy manages session state automatically

### Social Login Flow

1. User clicks social login button
2. User is redirected to social provider (Google, Apple, etc.)
3. User authorizes application
4. Provider redirects to Supabase with authorization code
5. Supabase creates user session and redirects to your app


## Supabase-Specific Features

Key features of the Supabase provider:

- **Row Level Security**: Database-level security that automatically filters data based on authenticated user
- **Real-time Subscriptions**: Live data updates that respect authentication context
- **Social Providers**: Built-in support for multiple OAuth providers
- **User Management**: Built-in user management APIs and dashboard

## Security Best Practices

- **Always use HTTPS** in production environments
- **Store keys securely** in user secrets or environment variables
- **Enable Row Level Security** on database tables containing user data
- **Configure email rate limiting** to prevent abuse
- **Validate tokens server-side** for sensitive operations
- **Monitor authentication logs** in Supabase Dashboard

## Troubleshooting

### Common Issues

**Invalid API Key**
- Verify anon key is correct and copied from Settings > API in your Supabase dashboard
- Check that the key hasn't been regenerated
- Ensure you're using the anon key, not service role key for client authentication

**Email Not Delivered**
- Check spam/junk folders for authentication emails
- Verify SMTP settings in Authentication > Settings
- Test with different email providers
- Check Supabase email quota limits

**Authentication Failed**
- Verify users exist and are not blocked in Supabase Dashboard
- Check that social providers are properly configured in Authentication > Providers
- Ensure redirect URLs match exactly in both Supabase and provider settings

**Row Level Security Issues**
- Check Row Level Security policies are correctly configured
- Verify `auth.uid()` matches expected user ID in your policies
- Test policies using the Supabase SQL editor
- Ensure user is properly authenticated before accessing data

## Related Documentation

- [Authentication Overview](Overview.md)
- [Auth0 Provider](Auth0.md)
- [Microsoft Entra Provider](MicrosoftEntra.md)