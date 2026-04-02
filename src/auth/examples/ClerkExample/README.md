# Clerk Example

This example demonstrates authentication using Clerk.

If running in production mode (`IVY_ENVIRONMENT=Production`), the example will look for configuration in a JSON file at the path stored in environment variable `IVY_CLERK_SECRETS_PATH`. This enables different secrets to be used for testing the development and production modes of Clerk.
