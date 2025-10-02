# Remove Branding

<Ingress>
Remove Ivy branding from your deployed projects with a valid Ivy Pro or Team subscription.
</Ingress>

The `ivy remove-branding` command allows users with a paid subscription to remove Ivy branding from their projects. This command configures the necessary license token in your project's .NET user secrets, which must then be included in your deployment environment.

## Prerequisites

Before using the remove-branding command, ensure you have:

1. **Paid Subscription** - A valid Ivy Pro or Team subscription
2. **Authentication** - Must be logged in with `ivy login`
3. **Ivy Project** - Must be run in a valid Ivy project directory

## Usage

```terminal
>ivy remove-branding
```

This command will:

- Verify you have a valid Ivy Pro or Team subscription
- Retrieve your license token from the Ivy billing service
- Set the `Ivy:License` configuration value in .NET user secrets
- Add the license configuration to your project's secrets manager

### Command Options

`--project-path <PATH>` or `-p <PATH>` - Specify the path to your project directory. Defaults to the current working directory.

```terminal
>ivy remove-branding --project-path /path/to/your/project
```

`--verbose` - Enable verbose output for detailed logging during the process.

```terminal
>ivy remove-branding --verbose
```

## Deployment Configuration

<Callout Type="Warning">
The `Ivy:License` configuration value must be included in your deployment environment for branding to be removed. The command only sets up local development secrets.
</Callout>

### Cloud Deployment

When using `ivy deploy` to simplify deployment, the license configuration will be automatically included in your deployment if it has been configured in your .NET user secrets.

### Manual Deployment

When deploying an Ivy project without using `ivy deploy`, your local .NET user secrets are not automatically transferred. In that case, you can configure your Ivy license by setting the environment variable or .NET user secret below.

```bash
# Environment variable
Ivy__License=your-unique-license-token

# User secret
Ivy:License=your-unique-license-token
```

> **Note:** If configuration is present in both .NET user secrets and environment variables, Ivy will use the values in **.NET user secrets over environment variables**.

To retrieve your license token, run `ivy remove-branding` locally, then look for `Ivy:License` in your user secrets. See [Check User Secrets](#check-user-secrets) below for more information.

## Verification

After running the command successfully, verify the configuration:

### Check User Secrets

```terminal
>dotnet user-secrets list
```

You should see:
```
Ivy:License = your-unique-license-token
```

### Local Testing

Run your project locally to verify branding has been removed:

```terminal
>dotnet watch
```

## Troubleshooting

### License Token Not Working

- Ensure the `Ivy:License` configuration is properly set in your deployment environment
- Verify your subscription is still active
- Re-run `ivy remove-branding` to refresh the license token

### Configuration Not Found

- Check that .NET user secrets are properly initialized
- Use `--verbose` flag for detailed logging

### Deployment Issues

- Confirm license environment variable or secret is correctly set in your deployment platform
- Verify the license token is not being truncated or modified

## Related Commands

- `ivy login` - Authenticate with your Ivy account
- `ivy deploy` - Deploy your project (includes license configuration)
- `ivy init` - Initialize a new Ivy project

## Subscription Management

To manage your Ivy subscription and check available features:

- Visit [https://ivy.app/pricing](https://ivy.app/pricing) to view plans
- Check your current subscription status in your Ivy account
- Contact support for subscription-related questions
