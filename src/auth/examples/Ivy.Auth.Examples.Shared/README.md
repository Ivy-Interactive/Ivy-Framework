# Ivy.Auth.Examples.Shared

This shared library contains reusable OAuth provider testing UI components for Ivy authentication example projects.

## Purpose

This library extracts common OAuth provider API testing code from individual example projects into reusable view classes. This eliminates code duplication and provides a consistent testing experience across all authentication provider examples.

## Components

### GoogleOAuthTestView

Tests Google OAuth integration with the following capabilities:
- **Get Google Profile**: Fetches user profile information from Google OAuth API
- **List Google Drive Files**: Retrieves the first 10 files from the user's Google Drive

**Usage:**
```csharp
new GoogleOAuthTestView(session)
```

### GitHubOAuthTestView

Tests GitHub OAuth integration with the following capabilities:
- **Get GitHub User**: Fetches the authenticated user's GitHub profile
- **Fetch My Repositories**: Retrieves the first 10 repositories for the authenticated user

**Usage:**
```csharp
new GitHubOAuthTestView(session, appName: "YourAppName")
```

### MicrosoftGraphOAuthTestView

Tests Microsoft Graph API integration with the following capabilities:
- **Get Profile**: Fetches user profile information from Microsoft Graph
- **List OneDrive Files**: Retrieves files from the user's OneDrive root folder

**Usage:**
```csharp
new MicrosoftGraphOAuthTestView(session)
```

## Integration Example

To use these components in an authentication example project:

1. Add a project reference to `Ivy.Auth.Examples.Shared`:
```xml
<ItemGroup>
  <ProjectReference Include="..\Ivy.Auth.Examples.Shared\Ivy.Auth.Examples.Shared.csproj" />
</ItemGroup>
```

2. Import the namespace in your MainApp.cs:
```csharp
using Ivy.Auth.Examples.Shared;
```

3. Use the view components when displaying OAuth provider sessions:
```csharp
oauthSessions.Value?.TryGetValue(OAuthProvider.Google, out var googleSession) == true
    ? new GoogleOAuthTestView(googleSession)
    : null
```

## Benefits

- **Code Reusability**: Write OAuth testing code once, use it across all example projects
- **Consistency**: Provides uniform OAuth API testing experience
- **Maintainability**: Updates to OAuth testing logic only need to be made in one place
- **Extensibility**: Easy to add new OAuth provider test views as needed

## Architecture

Each view class:
- Inherits from `ViewBase` to integrate with Ivy's UI framework
- Accepts an `IAuthTokenHandlerSession` as a constructor parameter
- Uses Ivy UI components (Button, Layout, Text) for rendering
- Manages its own state for API responses
- Handles errors gracefully and displays them to the user
