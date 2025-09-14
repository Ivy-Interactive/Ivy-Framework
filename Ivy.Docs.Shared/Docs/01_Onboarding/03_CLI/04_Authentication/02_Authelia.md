# Authelia Authentication Provider

<Ingress>
Secure your Ivy application with Authelia's self-hosted identity provider supporting LDAP and forward auth.
</Ingress>

## Overview

Authelia is an open-source authentication and authorization server providing comprehensive identity verification and access control features. It offers single sign-on and supports various authentication backends including LDAP and file-based users, making it ideal for self-hosted environments.

## Setting Up Your Authelia Server

Before using Authelia with Ivy, you must have a running Authelia instance. You can start with Authelia's [Get started](https://www.authelia.com/integration/prologue/get-started/) guide. Then, continue with the deployment instructions for your environment:

- [Docker](https://www.authelia.com/integration/deployment/docker/)
- [Kubernetes](https://www.authelia.com/integration/kubernetes/introduction/)
- [Bare-Metal](https://www.authelia.com/integration/deployment/bare-metal/)

## Adding Authentication

To set up Authelia Authentication with Ivy, run the following command and choose `Authelia` when asked to select an auth provider:

```terminal
>ivy auth add
```

You will be prompted to provide your Authelia server URL (e.g., `https://127.0.0.1:9091` or `https://auth.yourdomain.com`).

> **Note:** Authelia requires the use of HTTPS, even for local testing.

Your configuration will be stored securely in .NET user secrets. Ivy then finishes configuring your application automatically:

1. Adds the `Ivy.Auth.Authelia` package to your project.
2. Adds `server.UseAuth<AutheliaAuthProvider>();` to your `Program.cs`.
3. Adds `Ivy.Auth.Authelia` to your global usings.

### Advanced Configuration

#### Connection Strings

To skip the interactive prompts, you can provide configuration via a connection string:

```terminal
>ivy auth add --provider Authelia --connection-string "AUTHELIA_URL=https://auth.yourdomain.com"
```

For a list of connection string parameters, see [Configuration Parameters](#configuration-parameters) below.

#### Manual Configuration

When deploying an Ivy project without using `ivy deploy`, your local .NET user secrets are not automatically transferred. In that case, you can configure Authelia auth by setting environment variables or .NET user secrets. See [Configuration Parameters](#configuration-parameters) below.

> **Note:** If configuration is present in both .NET user secrets and environment variables, Ivy will use the values in **.NET user secrets over environment variables**.

For more information, see [Authentication Overview](Overview.md).

#### Configuration Parameters

The following parameters are supported via connection string, environment variables, or .NET user secrets:

- **AUTHELIA_URL**: Required. The base URL of your Authelia instance.

## Authentication Flow

1. User provides credentials directly in your Ivy application
2. Ivy sends credentials to your Authelia instance for validation
3. Authelia validates credentials against configured backend (file-based users, LDAP, etc.)
4. If valid, Authelia returns a session token
5. Ivy uses the session token for subsequent authenticated requests

## Authelia-Specific Features

Key features of the Authelia provider:

- **Self-hosted Control**: Complete control over your authentication infrastructure
- **Multiple Backends**: Supports file-based users, LDAP, Active Directory integration on the Authelia server
- **Direct Integration**: Ivy communicates directly with Authelia's API for credential validation
- **Granular Access Control**: Fine-grained rules based on users, groups, and resources

## Security Best Practices

- **Always use HTTPS** for all Authelia communications
- **Generate strong secrets** for JWT and session encryption keys
- **Use secure password hashing** (argon2id recommended)
- **Configure rate limiting** to prevent brute force attacks
- **Monitor authentication logs** for suspicious activity
- **Keep Authelia updated** to the latest version

## Troubleshooting

### Common Issues

**Connection Refused**
- Verify Authelia service is running and accessible
- Check firewall settings allow connections to your Authelia port
- Ensure network connectivity between your application and Authelia instance

**Configuration Issues**
- Verify your Authelia URL is correct and accessible from your Ivy application
- Check that Authelia is properly configured and running
- Ensure your Authelia instance has the required API endpoints enabled

**Authentication Failed**
- Check user credentials exist in your configured authentication backend
- Verify password hashing matches Authelia's configuration
- Ensure authentication backend (file, LDAP) is properly configured

## Related Documentation

- [Authentication Overview](Overview.md)
- [Auth0 Provider](Auth0.md)
- [Microsoft Entra Provider](MicrosoftEntra.md)