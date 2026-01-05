# Ivy.Auth.Clerk

An Ivy authentication provider for Clerk (https://clerk.com).

## Configuration

Add the following environment variables or user secrets:

```
Clerk:SecretKey=your_secret_key
Clerk:PublishableKey=your_publishable_key
```

Both keys must be for the same environment (either both `test` or both `live`).

## Usage

```csharp
var authProvider = new ClerkAuthProvider()
    .UseEmailPassword()
    .UseGoogle()
    .UseGithub()
    .UseMicrosoft();
```

## Supported Authentication Methods

- Email/Password
- Google OAuth
- GitHub OAuth
- Twitter OAuth
- Apple OAuth
- Microsoft OAuth
