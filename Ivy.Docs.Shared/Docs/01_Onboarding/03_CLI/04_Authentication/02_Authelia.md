# Authelia Authentication Provider

<Ingress>
Secure your Ivy application with Authelia's self-hosted identity provider supporting LDAP and forward auth.
</Ingress>

## Overview

Authelia is an open-source authentication and authorization server providing comprehensive identity verification and access control features. It offers single sign-on and supports various authentication backends including LDAP and file-based users, making it ideal for self-hosted environments.

## Setting Up Your Authelia Server

Before using Authelia with Ivy, you need a running Authelia instance. Here are the most common setup approaches:

### Option 1: Docker Compose (Recommended)

Create a `docker-compose.yml` file:

```yaml
version: '3.7'
services:
  authelia:
    image: authelia/authelia:latest
    container_name: authelia
    volumes:
      - ./authelia:/config
    ports:
      - "9091:9091"
    environment:
      - AUTHELIA_JWT_SECRET=your-jwt-secret-here
      - AUTHELIA_SESSION_SECRET=your-session-secret-here
      - AUTHELIA_STORAGE_ENCRYPTION_KEY=your-encryption-key-here
```

### Option 2: Binary Installation

1. **Download** from [Authelia releases](https://github.com/authelia/authelia/releases)
2. **Extract** the binary
3. **Create** a configuration file (see below)
4. **Run**: `./authelia --config /path/to/configuration.yml`

### Basic Configuration File

Create `/config/configuration.yml`:

```yaml
server:
  host: 0.0.0.0
  port: 9091

log:
  level: info

authentication_backend:
  file:
    path: /config/users_database.yml
    password:
      algorithm: argon2id

access_control:
  default_policy: deny
  rules:
    - domain: "localhost"
      policy: one_factor

session:
  name: authelia_session
  domain: localhost
  same_site: lax
  secret: your-session-secret

storage:
  local:
    path: /config/db.sqlite3

notifier:
  filesystem:
    filename: /config/notification.txt
```

### Create Users File

Create `/config/users_database.yml`:

```yaml
users:
  john:
    displayname: "John Doe"
    password: '$argon2id$v=19$m=65536,t=3,p=4$your-hashed-password-here'
    email: john.doe@example.com
    groups:
      - users

  admin:
    displayname: "Administrator"
    password: '$argon2id$v=19$m=65536,t=3,p=4$your-hashed-password-here'
    email: admin@example.com
    groups:
      - admins
      - users
```

**To hash passwords**, you can use Authelia's hash-password command:
```bash
authelia hash-password 'your-password-here'
```

### Getting Your Configuration

Once your Authelia server is running:

- **AUTHELIA_URL**: Your Authelia server URL (e.g., `http://localhost:9091` or `https://auth.yourdomain.com`)

## Adding Authentication

To set up Authelia Authentication with Ivy, run the following command and choose `Authelia` when asked to select an auth provider:

```terminal
>ivy auth add
```

You will be prompted to provide your Authelia server URL (e.g., `https://auth.yourdomain.com`).

Your configuration will be stored securely in .NET user secrets. Ivy then finishes configuring your application automatically:

1. Adds the `Ivy.Auth.Authelia` package to your project.
2. Adds `server.UseAuth<AutheliaAuthProvider>();` to your `Program.cs`.
3. Adds `Ivy.Auth.Authelia` to your global usings.

### Connection String Format

To skip the interactive prompts, you can provide configuration via a connection string parameter:

```terminal
>ivy auth add --provider Authelia --connection-string "AUTHELIA_URL=https://auth.yourdomain.com"
```

The connection string uses the following parameters:

- **AUTHELIA_URL**: Required. The base URL of your Authelia instance.

### Advanced Configuration

The following parameter can be manually set via .NET user secrets or environment variables:

- **AUTHELIA_URL**: The base URL of your Authelia instance. Set by `ivy auth add`.

If configuration is present in both .NET user secrets and environment variables, Ivy will use the values in .NET user secrets. For more information, see [Authentication Overview](Overview.md).

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