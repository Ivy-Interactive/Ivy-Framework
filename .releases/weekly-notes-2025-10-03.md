# Ivy Framework Weekly Notes - Week of 2025-10-03

## CLI Improvements

### Extended Default Timeouts

The default timeout for `ivy app create` and `ivy fix` commands has been increased from 120 to 360 seconds. This provides more time for longer-running operations without needing to manually specify the `--timeout` option.

```bash
# These commands now default to 360 seconds
ivy app create
ivy fix

# You can still override if needed
ivy app create --timeout 600
```

## Framework Enhancements

### Improved Refresh Token Security

The `BasicAuthProvider` now implements JWT-based refresh tokens instead of random string tokens. This provides several security and usability improvements:

- **Shorter JWT expiration**: Access tokens now expire after 15 minutes (previously 1 hour)
- **Longer refresh token validity**: Refresh tokens are valid for 24 hours but respect a 365-day maximum age from original authentication
- **Stateless refresh tokens**: No server-side storage required - all refresh token data is encoded in the JWT
- **Automatic token refresh**: If your current JWT is still valid, calling refresh returns the existing token

The refresh token includes security claims like `sid` (session ID), `auth_time`, and `max_age` to enforce session boundaries. The system automatically validates refresh tokens and rejects expired or invalid tokens.

```csharp
// Login returns both access token and refresh token
var authToken = await authProvider.LoginAsync(email, password);
// authToken.Jwt expires in 15 minutes
// authToken.RefreshToken expires in 24 hours (but max 365 days from login)

// Refresh your access token before it expires
var newToken = await authProvider.RefreshJwtAsync(authToken);
// Returns existing token if still valid, or generates new access token
// Refresh token is reused if within validity period
```

This change improves security by reducing the window of exposure for access tokens while maintaining a smooth user experience with long-lived refresh tokens.

### Improved Text Wrapping in Lists and Details

List items and detail widgets now properly handle long text that exceeds the available width. Previously, long product names or descriptions would overflow or be cut off with `whitespace-nowrap`. The widgets now use `whitespace-normal break-words` to wrap text across multiple lines while maintaining proper layout.

This improvement applies to:
- `ListWidget` - titles and subtitles now wrap naturally
- `DetailWidget` - detail values wrap instead of being truncated
- Dynamic row height measurement ensures rows expand to fit multi-line content

No code changes needed - existing list and detail widgets automatically benefit from this enhancement.

### JobScheduler: Continue On Child Failure

The `JobScheduler` now supports continuing execution when child jobs fail. Use the `WithContinueOnChildFailure()` method when building jobs to prevent parent job failure when a child job encounters an error.

```csharp
Job generateAppsJob = scheduler.CreateJob("Generate Apps")
    .WithAction((j, s, _, _) => GenerateAppsJobs(j, s, connectionName))
    .WithContinueOnChildFailure()  // Parent job continues even if child jobs fail
    .DependsOn(addDatabaseToProjectJob)
    .Build();
```

This is useful for scenarios where you want to process multiple independent operations and continue even if some fail, rather than stopping the entire job chain.

### IVY_ENVIRONMENT Variable

When deploying apps to AWS, Azure, GCP, or Sliplane, the `IVY_ENVIRONMENT` variable is now automatically set to "PRODUCTION". This allows your application to differentiate between local development and production environments.

```csharp
var environment = Environment.GetEnvironmentVariable("IVY_ENVIRONMENT");
if (environment == "PRODUCTION")
{
    // Production-specific configuration
}
```

### Service Description Support

Services can now implement the `IDescribableService` interface to provide descriptions when using the `--describe` flag. The `ServerDescriptionReader` now includes a `Description` property in the service metadata, making it easier to document and understand registered services.

### SQLite Volume Validation for Sliplane Deployments

When deploying applications using SQLite to Sliplane, the CLI now automatically validates that volume requirements are properly configured. This prevents deployment issues where SQLite databases would be lost on redeployment.

The deployment process now checks for:
- SQLite database connections in your application
- Proper `IVolume` service registration using `FolderVolume`
- Correct mount path configuration for production environments

If the volume is not properly configured, you'll receive clear error messages with instructions:

```csharp
// Required setup in Program.cs for SQLite on Sliplane
server.UseVolume(new FolderVolume(Ivy.Utils.IsProduction() ? "/app/data" : null));
```

The volume configuration is automatically included in the Sliplane service deployment, ensuring your SQLite database persists across deployments.

### BoolInput Size Support

The `BoolInput` widget now supports three size variants: Small, Medium (default), and Large. This applies to all boolean input variants including Checkbox, Switch, and Toggle, allowing you to adjust the input size to match your UI design needs.

```csharp
// Use helper methods for common sizes
boolState.ToBoolInput()
    .Label("Accept terms")
    .Small()  // Compact size

boolState.ToSwitchInput()
    .Label("Enable notifications")
    .Large()  // Prominent size

boolState.ToToggleInput(Icons.Star)
    .Label("Favorite")
    .Large();  // Works with toggle icons too

// Or specify size explicitly
boolState.ToBoolInput()
    .Size(Sizes.Medium)
    .Label("Remember me");
```

### DateRangeInput Size Support

The `DateRangeInput` widget now supports three size variants: Small, Medium (default), and Large. This allows you to adjust the input size to match your UI design needs.

```csharp
// Use helper methods for common sizes
dateRangeState.ToDateRangeInput()
    .Small()  // Compact size
    .Placeholder("Select date range");

dateRangeState.ToDateRangeInput()
    .Large()  // Prominent size
    .Format("yyyy-MM-dd");

// Or specify size explicitly
dateRangeState.ToDateRangeInput()
    .Size(Sizes.Medium)
    .Placeholder("Select date range");
```

## Bug Fixes

### Fixed Enum Serialization in SelectInput

Fixed a critical bug in `JsonEnumConverter` where enums with `Description` attributes were being serialized using their description values instead of their actual enum names. This caused errors when the frontend tried to send select input values back to the backend.

**Before:** An enum value like `DatabaseNamingConvention.SnakeCase` with description `"snake_case"` would serialize as `"snake_case"`, causing "Requested value 'snake_case' was not found" errors.

**After:** The same enum now correctly serializes as `"SnakeCase"` (the actual enum name), while still accepting both enum names and descriptions during deserialization for backward compatibility.

```csharp
public enum DatabaseNamingConvention
{
    [Description("PascalCase")]
    PascalCase,
    [Description("snake_case")]
    SnakeCase
}

// Serializes as "SnakeCase" (not "snake_case")
var json = JsonSerializer.Serialize(DatabaseNamingConvention.SnakeCase);

// Accepts both for deserialization
var value1 = JsonSerializer.Deserialize<DatabaseNamingConvention>("\"SnakeCase\"");   // ✓
var value2 = JsonSerializer.Deserialize<DatabaseNamingConvention>("\"snake_case\"");  // ✓ (backward compat)
```

This fix ensures SelectInput widgets work correctly with enums that have Description attributes.

### Fixed Hardcoded Min/Max in NumberInputWidget

The `NumberInputWidget` previously hardcoded `min` to 0 and `max` to 100, which prevented creating number inputs without bounds. These are now optional parameters, allowing for truly unbounded number inputs.

```csharp
// Previously, this would still enforce min=0 and max=100
numberState.ToNumberInput();

// Now works without bounds - no min/max constraints
numberState.ToNumberInput();

// You can still specify bounds when needed
numberState.ToNumberInput()
    .Min(10)
    .Max(500);

// Or specify just one bound
numberState.ToNumberInput()
    .Min(0);  // No maximum
```

This also fixes the slider variant to only display min/max labels when those bounds are actually specified.