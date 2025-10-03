> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

## Framework Improvements

### Fixed AuthService Token Storage

The `AuthService` now correctly maintains authentication token state across method calls. Previously, tokens were returned from `LoginAsync` and `HandleOAuthCallbackAsync` but not stored internally, causing subsequent operations like `GetUserInfoAsync` to fail. The service now properly:

- Stores tokens after successful login
- Stores tokens after OAuth callback
- Clears tokens on logout
- Uses stored tokens for user info retrieval

This ensures authentication state is maintained throughout your application session without requiring manual token management.

### Enhanced JWT Refresh Token Implementation

The `BasicAuthProvider` now uses JWT-based refresh tokens instead of random strings, providing better security and more control over token lifecycle. Key improvements include:

- **Shorter-lived access tokens**: Access tokens now expire after 15 minutes (previously 1 hour)
- **24-hour refresh tokens**: Refresh tokens are valid for 24 hours with proper JWT validation
- **Max age enforcement**: Refresh tokens include a `max_age` claim (365 days) to limit how long a session can be refreshed
- **Stateless validation**: Refresh tokens are now JWTs, eliminating the need for server-side storage

The refresh token automatically extends your session when the access token expires:

```csharp
// The auth provider now returns tokens with refresh capability
var authToken = await authProvider.LoginAsync(email, password);
// authToken.Jwt - Access token (15 min expiry)
// authToken.RefreshToken - Refresh token (24 hour expiry, 365 day max age)

// When access token expires, use refresh token to get a new one
var newToken = await authProvider.RefreshJwtAsync(authToken);
```

This change provides better security by reducing access token lifetime while maintaining user convenience through automatic session extension.

### JobScheduler: Continue on Child Failures

The JobScheduler now supports continuing execution even when child jobs fail. Use the `WithContinueOnChildFailure()` method when building jobs to enable this behavior.

```csharp
Job parentJob = scheduler.CreateJob("Parent Job")
    .WithAction((j, s, _, _) => CreateChildJobs(j, s))
    .WithContinueOnChildFailure()  // Parent job continues even if children fail
    .Build();
```

This is particularly useful for batch operations where you want to process all items even if some fail.

### Environment-Aware Service Descriptions

The `ServerDescriptionReader` now supports environment-specific service descriptions. You can specify which environment context to use when reading service descriptions from your application.

```csharp
// Read service descriptions with a specific environment
var description = await ServerDescriptionReader.ReadAsync(
    projectDirectory,
    environment: "PRODUCTION"
);
```

Services can now also provide custom descriptions by implementing the `IDescribableService` interface, and the `ServiceDescription` class includes an optional `Description` property for better documentation of your services.

### Automatic Production Environment Configuration

When deploying to cloud providers (AWS App Runner, Azure Container Apps, GCP Cloud Run, or Sliplane), the `IVY_ENVIRONMENT` variable is now automatically set to `PRODUCTION`. This allows your application to detect the deployment environment and adjust behavior accordingly.

## UI Improvements

### Lazy Loading for Embed Widget

The Embed widget now uses lazy loading to improve initial page load performance. Instead of loading all embed component code upfront, each platform's embed code (Twitter, Facebook, Instagram, TikTok, LinkedIn, Pinterest, GitHub, Reddit) is only loaded when you actually use an embed from that platform.

```csharp
// Component code is loaded on demand when the embed is rendered
new Embed("https://twitter.com/user/status/12345")  // Only loads Twitter embed code
```

### Enhanced Embed Widget Security

The Embed widget now includes comprehensive input sanitization to protect against malicious URLs and injection attacks. All embedded content from social media platforms (YouTube, Twitter/X, Facebook, Instagram, TikTok, LinkedIn, Pinterest, GitHub, and Reddit) is now validated and sanitized before rendering:

```csharp
// URLs are automatically validated and sanitized
new Embed("https://youtube.com/watch?v=12345")

// Invalid URLs or dangerous protocols are rejected
new Embed("javascript:alert('xss')")  // Will show "Invalid URL provided."
```

### Optimized External Script Loading

The Embed widget now intelligently manages external scripts to prevent duplicate loading when displaying multiple embeds from the same platform. When you add multiple Instagram, TikTok, LinkedIn, or Reddit embeds to a page, the platform's embed script is loaded only once and reused across all instances.

### Improved Facebook Embed Display

Facebook embeds now feature a more compact, modern design that adapts to your application's theme. The new layout presents Facebook posts in a clean card format similar to GitHub repository cards, with better visual hierarchy and proper theme integration. The embed automatically respects your app's color scheme (light/dark mode) and provides a cleaner interface for viewing Facebook content.

### Improved Pinterest Embed Display

Pinterest embeds now feature the same modern, compact design as other social media embeds. The new card-based layout replaces the old Pinterest widget with a cleaner interface that:

### Improved LinkedIn Embed Display

LinkedIn embeds now feature a more responsive design that adapts seamlessly across different screen sizes. The new implementation:

### Improved GitHub Embed Display

GitHub embeds now feature a compact, modern card design that adapts to your application's theme. The new implementation:

### Improved Twitter/X Embed Display

Twitter embeds now feature the same modern, compact design as other social media embeds. The new card-based layout replaces the Twitter widget with a cleaner interface that:

### Improved Embed Error Handling

The Embed widget now features comprehensive error handling to gracefully manage failed embeds and invalid URLs. When an embed fails to load or encounters an error, users see a clean error card instead of a broken component:

```csharp
// Automatically handles errors for all platforms
new Embed("https://youtube.com/invalid-url")
// Shows: "YouTube Embed Error - Failed to load embed. Please try again or visit the link directly."

new Embed("not-a-valid-url")
// Shows: "Unsupported URL - This URL is not supported for embedding. Please visit the link directly."
```

### Interactive Card Widget

The Card widget now supports click handling and hover effects, making it easier to create interactive UI elements:

```csharp
// Simple click handler
new Card("Click me for more details")
    .Title("Product Card")
    .Description("Click to view details")
    .HandleClick(() => client.Navigate("/product/123"));
```

When you add a click handler using `HandleClick()`, the card automatically applies hover effects with pointer cursor and a subtle translate animation (`CardHoverVariant.PointerAndTranslate`). You can customize the hover behavior using the `Hover()` method to choose between no styling (`None`), pointer only (`Pointer`), or pointer with animation (`PointerAndTranslate`).

### Audio Recorder Size Variants

The AudioRecorder widget now supports consistent size variants (Small, Medium, Large) for better UI flexibility.

```csharp
// Small size for compact display
new AudioRecorder("Start recording", "Recording audio...").Small()

// Medium size (default)
new AudioRecorder("Start recording", "Recording audio...")

// Large size for prominent display
new AudioRecorder("Start recording", "Recording audio...").Large()

// Or specify size explicitly
new AudioRecorder("Start recording", "Recording audio...").Size(Sizes.Large)
```

The size variant affects the padding, button size, text size, and microphone/stop icons for a cohesive appearance at different scales.

### Text Input Size Variants

Text input widgets now support consistent size variants (Small, Medium, Large) across all text input types for better UI flexibility.

### Improved Text Overflow Handling

List items and detail widgets now handle long text with smart truncation. Single-line content (titles, subtitles, and non-multiline details) automatically truncate with an ellipsis when they exceed the available width, preventing layout breaks and maintaining consistent row heights. For detail widgets, you can enable multiline mode to allow text wrapping when you need to show full content:

```csharp
// Single-line mode (default) - truncates with ellipsis
new Detail("Title", "A very long description that will be truncated...")

// Multiline mode - wraps text naturally
new Detail("Description", "A long description that needs to be fully visible...")
    .MultiLine()
```

Button links (Link and Inline variants) now handle long text gracefully with automatic truncation and tooltip support. When button text is too long to fit, it truncates with an ellipsis and shows the full text in a tooltip on hover, preventing layout breaks while keeping the full text accessible.

### Fixed Blade Widget Scrolling

The blade widget now correctly handles scrolling in nested containers, ensuring smooth and predictable scroll behavior when working with content that exceeds the visible area.

### Optional Min/Max for Number Inputs

Number input widgets no longer enforce hardcoded minimum (0) and maximum (100) values. The `min` and `max` props are now optional, giving you full control over value constraints.

The slider variant now also respects these optional constraints, hiding the min/max labels when not specified.

### Fixed DateTimeInput with DateTimeOffset

The DateTimeInput widget now correctly handles `DateTimeOffset` and nullable `DateTimeOffset?` state bindings. Previously, DateTimeOffset values weren't properly parsed and converted, causing binding issues. The widget now:

- Properly parses DateTimeOffset values from strings
- Correctly handles nullable DateTimeOffset states
- Prevents text overflow with proper truncation and icon sizing
- Fixes icon overlap issues where the calendar icon could shrink and text could overlap with clear/invalid icons
- Displays placeholder text in normal color instead of greyed-out when value is null

```csharp
// DateTimeOffset binding
var dateTimeOffsetState = UseState(DateTimeOffset.Now);
dateTimeOffsetState.ToDateTimeInput().Variant(DateTimeInputs.DateTime)

// Nullable DateTimeOffset binding - placeholder now shows in normal color
var nullableDateTimeOffsetState = UseState<DateTimeOffset?>(() => null);
nullableDateTimeOffsetState.ToDateTimeInput().Variant(DateTimeInputs.DateTime)
```

### Image Widget: Multiple Source Types

The Image widget now supports multiple image source types, giving you flexibility in how you provide images to your application:

```csharp
// External URLs - images hosted on external servers
new Image("https://example.com/image.jpg")

// Local files - images stored in your application's file system
new Image("/images/logo.png")

// Data URIs - Base64-encoded images embedded directly in the code
var dataUri = "data:image/png;base64,iVBORw0KGgoAAAANS...";
new Image(dataUri)
```

This flexibility allows you to choose the best image delivery method for your use case, whether it's external hosting, local assets, or embedded data.

## CLI Improvements

### Extended Default Timeouts

The default timeout for `ivy app create` and `ivy fix` commands has been increased from 120 seconds to 360 seconds (6 minutes). This provides more time for complex operations to complete without timing out.

```bash
# Both commands now default to 360 seconds
ivy app create MyApp

# You can still override the timeout if needed
ivy app create MyApp --timeout 600
```

### Sliplane Deployment: SQLite Volume Validation

When deploying applications with SQLite databases to Sliplane, the CLI now automatically validates that proper volume configuration is in place. This ensures your SQLite database persists across deployments and prevents data loss.

The deployment process will verify:
- That your application registers an `IVolume` service using `FolderVolume`
- That the mount path is configured for production environments

To deploy SQLite apps to Sliplane, configure your volume in `Program.cs`:

```csharp
server.UseVolume(new FolderVolume(
    Ivy.Utils.IsProduction() ? "/app/data" : null
));
```

The CLI will automatically create and mount the volume during deployment, ensuring your database file is persisted.