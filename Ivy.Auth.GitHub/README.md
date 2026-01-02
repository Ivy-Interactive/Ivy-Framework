# Ivy.Auth.GitHub

GitHub authentication provider for Ivy Framework using OAuth 2.0.

## Features

- GitHub OAuth 2.0 authentication flow
- User information retrieval from GitHub API
- Access token validation
- Support for GitHub user email addresses

## Configuration

Add the following configuration to your `appsettings.json` or environment variables:

```json
{
  "GitHub": {
    "ClientId": "your_github_client_id",
    "ClientSecret": "your_github_client_secret",
    "RedirectUri": "https://your-app.com/auth/github/callback"
  }
}
```

## GitHub App Setup

1. Go to GitHub Settings > Developer settings > OAuth Apps
2. Click "New OAuth App"
3. Fill in the application details:
   - **Application name**: Your application name
   - **Homepage URL**: Your application URL
   - **Authorization callback URL**: The redirect URI configured above
4. Copy the Client ID and Client Secret to your configuration

## Usage

1. Register the HttpClient factory in your DI container:

```csharp
builder.Services.AddHttpClient("GitHubAuth", client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "Ivy-Framework");
    client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
    client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
});
```

2. Create and configure the GitHub auth provider:

```csharp
using Ivy.Auth.GitHub;

// Create and configure the GitHub auth provider
var authProvider = new GitHubAuthProvider(httpClientFactory, configuration)
    .UseGitHub();

// Use with Ivy server
var server = new Server()
    .UseAuth(authProvider);
```

## Required Scopes

The GitHub OAuth app requests the following scopes:

- `user:email` - Access to user's email addresses

## Notes

- GitHub OAuth tokens are long-lived and don't have refresh tokens
- The provider automatically handles email verification and primary email selection
- User information includes GitHub username, display name, and avatar URL
