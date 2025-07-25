# Ivy.Auth.Github

GitHub authentication provider for the Ivy Framework.

## Features

- GitHub OAuth 2.0 authentication
- GitHub API integration via Octokit
- Token validation and refresh
- User profile retrieval
- Support for specifying GitHub services/scopes

## Usage

```csharp
var authProvider = new GitHubAuthProvider()
    .UseGithub(new[] { "user:email", "read:user" });
```

## Configuration

Set the following environment variables or user secrets:

- `GITHUB_CLIENT_ID`: Your GitHub OAuth app client ID
- `GITHUB_CLIENT_SECRET`: Your GitHub OAuth app client secret

## Dependencies

- Octokit: GitHub API client
- Microsoft.AspNetCore.Authentication.OAuth: OAuth 2.0 middleware
- Microsoft.Extensions.Http: HTTP client factory