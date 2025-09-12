# Ivy Framework Weekly Notes - Week of 2025-09-12

## CLI Improvements

### Database Generation Improvements

#### Skip Debug Option

The `ivy db generate` command now supports a `--skip-debug` flag that allows you to skip the automatic debug commands during database generation. This can speed up the generation process when you don't need the additional debugging steps.

```bash
# Generate database without running debug commands
ivy db generate --skip-debug
```

#### Enhanced Entity Framework Support

The database generator now automatically includes essential Entity Framework global using directives in generated projects. This means you no longer need to manually add common EF Core namespaces to your database models and configurations.

The following namespaces are now automatically available in all generated database files:

- `System.ComponentModel.DataAnnotations` - for data annotations like `[Key]`, `[Required]`
- `System.ComponentModel.DataAnnotations.Schema` - for schema attributes like `[Table]`, `[Column]`
- `Microsoft.EntityFrameworkCore` - core EF functionality
- `Microsoft.EntityFrameworkCore.Metadata.Builders` - for fluent API configuration
- `Microsoft.EntityFrameworkCore.Storage.ValueConversion` - for value converters

This improvement reduces boilerplate code and makes working with Entity Framework in generated database projects more streamlined:

```csharp
// Before: Had to add using statements manually
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

[Table("Users")]
public class User
{
    [Key]
    public int Id { get; set; }
}

// After: These namespaces are globally available
[Table("Users")]
public class User
{
    [Key]
    public int Id { get; set; }
}
```

#### Custom Session ID Support

The database generator now accepts an optional `SessionId` parameter, giving you control over the session identifier used during database generation. Previously, a new GUID was always generated for each session. Now you can specify your own session ID, which is useful for:

- Maintaining consistency across related database operations
- Debugging and tracking specific generation sessions
- Integrating with external systems that require specific session identifiers

```csharp
// Using the new optional SessionId parameter
var args = new DatabaseGeneratorArgs(
    Namespace: "MyApp",
    ProjectDirectory: "./src",
    AccessToken: "token",
    AgentServer: "server",
    SessionId: myCustomGuid  // Optional - defaults to new GUID if not provided
);
```

### Cleaner Version Display

The `ivy --version` command now displays cleaner version numbers by removing unnecessary trailing ".0" from the output.

### Docker Support in Project Templates

The `ivy init` command now correctly updates Dockerfile references when creating a new project with a custom namespace. Previously, Dockerfiles in generated projects would still reference the template namespace, which could cause build failures. Now, both the `.csproj` and `.dll` references in Dockerfiles are automatically updated to match your project's namespace, ensuring Docker builds work out of the box.

### Separate Token Storage for Staging Environments

The CLI now stores authentication tokens separately for staging and production environments. When using the `--staging` flag with CLI commands, your staging authentication tokens are stored in a different file (`.jwt.staging`) from production tokens (`.jwt`). This prevents token conflicts and allows you to seamlessly switch between staging and production environments without having to re-authenticate each time.

```bash
# Login to staging environment - tokens stored separately
ivy login --staging

# Login to production - uses different token storage
ivy login
```

This improvement makes it easier to work with multiple environments during development and testing.

### App Removal Command

The CLI now includes a new `ivy app remove` command that makes it easy to remove Ivy apps from your project. This command provides multiple ways to delete apps and their associated files:

```bash
# Remove a specific app by name
ivy app remove --name MyApp

# Interactive mode - select from a list of existing apps
ivy app remove

# Remove all apps at once
ivy app remove --all
```

The command intelligently identifies and removes all related files for an app, including:

- The main app file (e.g., `MyApp.cs`)
- All associated view files in the Views directory (e.g., `MyApp.HomeView.cs`, `MyApp.AboutView.cs`)

Before deletion, the command displays all files that will be removed and asks for confirmation, ensuring you don't accidentally delete important code. The interactive mode is particularly useful when you have multiple apps and want to quickly select which one to remove from a list.

### Claude Code Integration for Build Error Fixing

The `ivy fix` command now supports using Claude Code as an alternative AI debugging tool for fixing build errors. This provides developers with more flexibility in choosing their preferred AI assistant for debugging .NET build issues.

You can enable Claude Code debugging in two ways:

```bash
# Use the command-line option
ivy fix --use-claude-code

# Or set an environment variable
export IVY_FIX_USE_CLAUDE_CODE=true
ivy fix
```

### Telemetry Upload for Fix Command

The `ivy fix` command now supports optional telemetry upload to help improve the debugging experience. When enabled, the command will upload an anonymized snapshot of your project (excluding `.git`, `bin`, and `obj` folders) for analysis. This helps the Ivy team understand common build issues and improve the fix command's effectiveness.

To enable telemetry upload, set the environment variable:

```bash
# Enable telemetry upload for ivy fix
export IVY_FIX_UPLOAD_TELEMETRY=true
ivy fix
```

You can manage this setting using the new debug commands:

```bash
# Enable telemetry upload
ivy debug enable-ivy-fix-upload-telemetry

# Disable telemetry upload
ivy debug disable-ivy-fix-upload-telemetry
```

## Deployment Features

### Sliplane Deployment Provider

The Ivy CLI now includes built-in support for deploying applications to Sliplane, a modern container hosting platform. This new deployment provider makes it easy to deploy your Ivy applications directly from the command line.

To deploy to Sliplane, use the standard deploy command with the Sliplane provider:

```bash
# Deploy your Ivy app to Sliplane
ivy deploy --provider sliplane
```

The Sliplane deployment provider offers several key features:

- **GitHub Integration**: Automatically detects and validates your GitHub repository (currently supports public repositories only)
- **Secure Credentials**: Stores your Sliplane API key and organization ID securely using .NET user secrets
- **Interactive Setup**: Guides you through selecting projects and servers, or creating new ones if needed
- **Docker Support**: Automatically generates Ivy-optimized Dockerfile and .dockerignore if not present
- **Environment Variables**: Automatically transfers your .NET user secrets as secure environment variables in Sliplane
- **Auto-Deploy**: Option to enable automatic deployments on future commits to your selected branch

## Chart Widget Improvements

### Consistent Chart Margins

All chart types (Line, Bar, Area, and Pie charts) now have consistent margin handling with improved visibility. The chart margins have been adjusted from zero to 10 pixels on all sides across all chart widgets, ensuring that chart elements, labels, and data points are no longer cut off at the edges.

## Database Generation Toolkit Enhancements

### Advanced Enum Metadata Support

The Database Generator Toolkit now includes powerful `EnumMetadata` utilities that provide enhanced enum handling capabilities for database models and serialization scenarios. This new feature makes working with enums in Entity Framework and API serialization much more robust.

The new `EnumMetadata<TEnum>` class provides:

- Automatic extraction of `DisplayAttribute` metadata including names, descriptions, and display order
- Support for resource-based localization through `ResourceType`
- Built-in serialization/deserialization helpers for consistent enum string representations
- Cached metadata for optimal performance

Here's how to use the new enum metadata features:

```csharp
// Define an enum with display attributes
public enum UserRole
{
    [Display(Name = "Administrator", Description = "Full system access", Order = 1)]
    [EnumMember(Value = "admin")]
    Admin = 1,
    
    [Display(Name = "Standard User", Description = "Limited access", Order = 2)]
    [EnumMember(Value = "user")]
    User = 2,
    
    [Display(Name = "Guest", Description = "Read-only access", Order = 3)]
    [EnumMember(Value = "guest")]
    Guest = 3
}

// Access enum metadata
var adminMetadata = EnumMetadata<UserRole>.Value(UserRole.Admin);
Console.WriteLine(adminMetadata.DisplayName); // "Administrator"
Console.WriteLine(adminMetadata.Description); // "Full system access"

// Get all enum values in display order
var allRoles = EnumMetadata<UserRole>.Values; // Returns ordered collection

// Serialize/deserialize enums consistently
string serialized = EnumMetadata.Serialize(UserRole.Admin); // "admin"
UserRole deserialized = EnumMetadata.Deserialize<UserRole>("admin"); // UserRole.Admin

// Simple string conversion
string displayName = EnumMetadata.ToString(UserRole.Admin); // "Administrator"
```

This enhancement is particularly useful for:

- Creating dropdown lists with properly localized display names
- Serializing enums to consistent string values in APIs
- Maintaining enum ordering in UI components
- Supporting multi-language applications through resource files

## Widget Enhancements

### Table Widget Multiline Support

The Table widget now supports multiline display for cell content, making it easier to display longer text within table cells without truncation.

You can enable multiline display for specific columns using the `MultiLine()` method on the TableBuilder:

```csharp
var products = new[] 
{
    new { Sku = "1234", Name = "T-Shirt", Description = "High quality cotton T-shirt with a comfortable fit and durable construction" },
    new { Sku = "1235", Name = "Jeans", Description = "Classic denim jeans with a modern cut, perfect for everyday wear" }
};

var table = products.ToTable()
    .MultiLine(e => e.Description)  // Enable multiline for the Description column
    .Width(Size.Full());
```

When multiline is enabled for a column:

- Cell content displays with word wrapping instead of being truncated
- Text breaks naturally at word boundaries using `break-all` CSS for better handling of very long text
- The cell height automatically adjusts to accommodate the content
- Other cells in the row maintain their standard single-line format with proper `whitespace-nowrap` to prevent unintended wrapping

The multiline implementation has been improved to better handle edge cases:

- Multiline cells now use `break-all` to ensure even extremely long words or URLs wrap properly
- Single-line cells explicitly use `whitespace-nowrap` for cleaner display
- The `min-w-0` class is now only applied to single-line cells to prevent layout issues

### Details Widget Improvements

#### Multiline Support

The Details widget now supports multiline display for long text values with improved layout behavior. When multiline mode is enabled, the widget switches to a vertical layout where labels appear above their values, providing better readability for lengthy content.

The new `MultiLine()` method allows you to specify which fields should use multiline display:

```csharp
// Create a details view with multiline support for specific fields
var person = new 
{
    FirstName = "John",
    LastName = "Very Long Last Name That Would Normally Be Truncated",
    Bio = "A lengthy biography text that needs more space to display properly..."
};

// Enable multiline for specific fields
var details = person.ToDetails()
    .MultiLine(x => x.LastName)
    .MultiLine(x => x.Bio)
    .RemoveEmpty();

return new Card(details);
```

The multiline fields will:

- Display in a vertical layout with labels above values
- Provide left-aligned text for better readability of long content
- Remove maximum width constraints for multiline values
- Maintain proper spacing and alignment with other detail fields

#### Label Overflow Handling

The Details widget now properly handles overflow for long label text. Labels in detail views now use text truncation with ellipsis when they exceed the available space, preventing layout issues when displaying fields with lengthy names. This ensures that both single-line and multiline detail items maintain proper alignment and visual consistency, even when field labels are unexpectedly long.

## UI Component Improvements

### Resizable Panel Handle Enhancement

The resizable panel handles now feature a cleaner, more subtle appearance with hover-activated visibility. The resize handles remain hidden until you hover over them, reducing visual clutter while maintaining full functionality.

### Header Layout Widget Improvements

The Header Layout widget now properly fills the available height of its container. Resolved several scroll issues.

### Footer Layout Border Fix

The Footer Layout widget now displays its top border correctly across the full width of the footer.

### Code Input Widget Scrollbar Fix

The Code input widget now properly sizes itself to prevent unnecessary horizontal scrollbars from appearing.

### Muted Text Typography Update

Muted text elements now use a smaller, more subtle font size for better visual hierarchy.

### Improved Tooltip Component for Text Overflow

TextBlock widgets now use a proper React tooltip component instead of the native browser tooltip for displaying truncated text. This enhancement provides a more polished and consistent user experience across the framework.

The new tooltip implementation:

- `Text.Block` if overflown now displays a tooltip, used in Details and Tables.

## New Audio Widgets

### Audio Player Widget

The framework now includes a new `Audio` widget for playing audio content directly in your Ivy applications. This widget provides a simple way to embed audio players with full browser control support.

The Audio widget supports common audio formats (MP3, WAV, OGG, AAC, M4A) and offers a fluent API for configuration:

```csharp
// Basic audio player with default controls
var audio = new Audio("path/to/audio.mp3");

// Looping background music with autoplay
var backgroundMusic = new Audio("music.mp3")
    .Loop(true)
    .Autoplay(true)
    .Muted(true)  // Muted autoplay is more likely to be allowed by browsers
    .Preload("auto");

// Custom sized audio player
var customAudio = new Audio("podcast.mp3")
    .Width(Size.Fraction(0.5f))
    .Height(Size.Units(12));

// Audio without visible controls (for programmatic control)
var hiddenAudio = new Audio("notification.mp3")
    .Controls(false)
    .Muted(true);
```

Key features:

- **Browser Controls**: Shows standard audio controls by default (play/pause, volume, seek bar)
- **Autoplay Support**: Can start playing automatically when loaded (works best when muted)
- **Loop Playback**: Option to continuously repeat the audio
- **Preload Strategies**: Control how the browser loads audio data ("none", "metadata", or "auto")
- **Custom Sizing**: Full control over the widget dimensions
- **Programmatic Control**: Hide controls for custom UI implementations

The widget automatically handles URL resolution, supporting both absolute URLs and relative paths that are resolved against your Ivy application host.

### AudioRecorder Widget

The framework now includes an `AudioRecorder` widget that enables users to record audio directly from their microphone. This widget provides a flexible recording interface with support for real-time uploads and chunked streaming.

```csharp
// Basic audio recorder with upload endpoint
public class AudioRecorderExample : ViewBase
{
    public override object? Build()
    {
        var uploadUrl = this.UseUpload(
            fileBytes => {
                // Process uploaded audio bytes
                Console.WriteLine($"Received {fileBytes.Length} bytes");
            },
            "application/octet-stream",
            "uploaded-audio"
        );

        return new AudioRecorder("Start recording", "Recording audio...")
            .UploadUrl(uploadUrl.Value)
            .ChunkInterval(3000); // Upload chunks every 3 seconds
    }
}

// Simple recorder without upload
var simpleRecorder = new AudioRecorder("Click to record");

// Recorder with custom MIME type
var webmRecorder = new AudioRecorder()
    .Label("Record Audio")
    .MimeType("audio/webm");

// Disabled recorder
var disabledRecorder = new AudioRecorder("Recording disabled", disabled: true);
```

Key features:

- **Real-time Upload**: Automatically uploads audio to specified endpoints
- **Chunked Streaming**: Optional chunked uploads for long recordings with configurable intervals
- **Visual Feedback**: Shows volume levels during recording with animated background
- **Recording Timer**: Displays elapsed time while recording
- **Flexible MIME Types**: Supports various audio formats (webm, wav, etc.) - Note: MP3 recording is not supported by Chrome
- **Custom Labels**: Separate labels for idle and recording states

The widget provides a clean, button-based interface with visual indicators for recording status and automatic handling of microphone permissions.

## Form Builder Improvements

### Custom Label Preservation for Boolean Inputs

The FormBuilder now correctly preserves custom labels for boolean inputs.

```csharp
// Before: Custom label would be ignored and replaced with "Is Active"
var form = new FormBuilder<UserModel>()
    .Field(x => x.IsActive, field => field
        .Label("User Account Status")); // This label was being overridden

// After: Your custom label is now preserved
var form = new FormBuilder<UserModel>()
    .Field(x => x.IsActive, field => field
        .Label("User Account Status")); // Now correctly shows "User Account Status"
```

## Breaking Changes

*No breaking changes this week.*

## Database Diagram Improvements

### Enhanced DBML Canvas Visual Feedback

The DBML Canvas widget now provides improved visual feedback for database diagram interactions. The hover and selection states have been refined with better border handling:

- **Improved Border Targeting**: Visual states now apply directly to the database table nodes rather than the container, ensuring borders appear exactly where expected
- **Consistent Border Width**: All interaction states now use a consistent 2px border width for better visual clarity
- **Theme-Aware Hover Effect**: Nodes now use the theme's `--muted-foreground` color for hover borders, providing better visibility across light and dark themes
- **Selection State**: Selected nodes display with a distinct primary color border, clearly indicating the active selection
- **Cleaner CSS**: Removed unnecessary `!important` declarations, allowing for easier customization and style overrides
- **Smoother Transitions**: Interaction states now transition more quickly (0.1s) for more responsive visual feedback

These improvements make working with database diagrams more intuitive, with clearer visual cues for interaction states that adapt properly to both light and dark themes.

## Documentation Improvements

### Enhanced API Documentation Copying

The framework's documentation system now properly handles copying of expandable sections in API documentation. When you copy API content that includes collapsible/expandable sections, the header text of these sections is now correctly included in the copied content with proper markdown formatting.
