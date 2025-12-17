# Ivy Framework Weekly Notes - Week of 2025-12-17

## Breaking Changes

### Namespace Simplification

The `AppAttribute` and `ViewBase` classes have been moved to the root `Ivy` namespace for a simpler, cleaner import structure:

**What changed:**
- `AppAttribute` moved from `Ivy.Apps` to `Ivy`
- `ViewBase` moved from `Ivy.Core` to `Ivy`

**Migration:**

Update your using statements:

```csharp
// Old imports
using Ivy.Apps;
using Ivy.Core;

// New imports
using Ivy;
```

If you have the root `Ivy` namespace already imported, no changes are needed. Your existing code will continue to work, as the old namespaces still contain the classes (they're just now also available in the root namespace).

```csharp
// This now works with just: using Ivy;
[App(title: "My App", icon: Icons.Home)]
public class MyApp : ViewBase
{
    public override object Build()
    {
        // Your implementation
    }
}
```

This simplification makes it easier to get started with Ivy and reduces the number of using statements needed in most files.

## Authentication Improvements

### HTTP Tunneling for Authentication

Authentication providers can now make HTTP requests through the frontend using a new tunneling system. This enables auth providers to communicate with external APIs securely by routing requests through the client's browser, which is particularly useful for OAuth flows and development mode scenarios where the backend can't directly reach certain endpoints.

The implementation includes:
- `HttpTunnelingController` for handling tunneled requests
- `TunneledHttpMessageHandler` that integrates with .NET's `HttpClient`
- Automatic request/response serialization and header forwarding
- 30-second timeout with proper cancellation handling

This infrastructure is used internally by authentication providers and requires no configuration from your end.

### New Clerk Authentication Provider

Ivy now supports Clerk as an authentication provider, bringing modern authentication features including OAuth social logins, passwordless authentication, and comprehensive user management.

**Setup:**

```csharp
// Configure in your Program.cs
server.UseAuth<ClerkAuthProvider>(provider => provider
    .UseEmailPassword()
    .UseGoogle()
    .UseGithub()
    .UseMicrosoft()
    .UseApple()
    .UseTwitter()
);
```

**Key features:**
- Email/password and username/password authentication
- Social logins (Google, GitHub, Microsoft, Apple, Twitter)
- Separate development and production environments for safe testing
- JWT-based session tokens with automatic refresh
- Built-in development OAuth credentials for quick local setup
- Session management across multiple tabs and devices

**Configuration:**

Set your Clerk API keys using environment variables or .NET user secrets:

```bash
Clerk:SecretKey=sk_test_...        # or sk_live_... for production
Clerk:PublishableKey=pk_test_...   # or pk_live_... for production
```

The provider automatically detects whether you're using development or production keys and adjusts its behavior accordingly. Development keys (`sk_test_*`, `pk_test_*`) include built-in OAuth credentials, while production keys require custom OAuth app configuration for each social provider.

See the [Clerk authentication documentation](https://docs.ivy-framework.com/authentication/clerk) for detailed setup instructions.

### Multi-Tab Authentication Synchronization

Authentication now works seamlessly across multiple tabs and windows. When you sign in or sign out in one tab, all other tabs from the same browser automatically sync:

- **Sign in once, authenticated everywhere**: Sign in on one tab and all your other open tabs instantly get authenticated without manual refresh
- **Sign out once, logged out everywhere**: Logging out in one tab immediately logs you out across all tabs for better security
- **Automatic session recovery**: Opening a new tab picks up your existing authentication state

This works by tracking a machine ID and coordinating authentication state across all sessions from the same browser.

**Implementation improvements**: The authentication synchronization system has been refactored to use a secure cookie-based approach instead of storing tokens directly in the frontend. This improves security by keeping sensitive authentication data HTTP-only and prevents token exposure to client-side JavaScript.

**Cookie security update**: Authentication cookies now use `SameSite=Lax` instead of `SameSite=Strict`. This change allows authentication to work correctly with OAuth callback flows where users are redirected from external identity providers back to your application. With the Lax setting, cookies are sent on top-level navigation (like OAuth redirects) while still providing CSRF protection for most scenarios.

### Improved Token Lifetime Management

The authentication system now provides more comprehensive token lifetime information through the new `TokenLifetime` type:

```csharp
public record TokenLifetime(DateTimeOffset? Expires = null, DateTimeOffset? NotBefore = null);
```

**What changed:**
- `GetAccessTokenExpirationAsync()` renamed to `GetAccessTokenLifetimeAsync()`
- Returns `TokenLifetime?` instead of `DateTimeOffset?`
- Includes both expiration time and validity start time

This allows for smarter token refresh logic. For tokens with short lifetimes (less than 3 minutes), Ivy now uses proportional renewal margins (1/6 of token lifetime) instead of a fixed 2-minute margin. This prevents tokens from expiring before they can be refreshed:

```csharp
// For a 60-second token, renewal happens at 50 seconds instead of attempting at -60 seconds
TimeSpan renewalMargin = tokenDuration < TimeSpan.FromMinutes(3)
    ? tokenDuration / 6
    : TimeSpan.FromMinutes(2);
```

**For custom auth provider implementations**, update your implementation:

```csharp
// Old way
public Task<DateTimeOffset?> GetAccessTokenExpirationAsync(IAuthSession authSession, CancellationToken cancellationToken)
{
    return Task.FromResult<DateTimeOffset?>(expirationTime);
}

// New way
public Task<TokenLifetime?> GetAccessTokenLifetimeAsync(IAuthSession authSession, CancellationToken cancellationToken)
{
    return Task.FromResult<TokenLifetime?>(new TokenLifetime(expirationTime));
}
```

### New `IAuthSession` Interface

The authentication system now uses `IAuthSession` instead of passing tokens directly. This provides better encapsulation and type safety:

```csharp
public interface IAuthSession
{
    AuthToken? AuthToken { get; set; }
    string? AuthSessionData { get; set; }
    HttpMessageHandler HttpMessageHandler { get; set; }
}
```

**For custom auth provider implementations**, you'll need to update your method signatures:

```csharp
// Old way
public Task<AuthToken?> LoginAsync(string email, string password, CancellationToken cancellationToken)
{
    // Your implementation
}

// New way
public Task<AuthToken?> LoginAsync(IAuthSession authSession, string email, string password, CancellationToken cancellationToken)
{
    // Access token via authSession.AuthToken
    // Store additional session data in authSession.AuthSessionData
}
```

All `IAuthProvider` methods now receive an `IAuthSession` parameter instead of raw tokens. This applies to:
- `LoginAsync()`
- `LogoutAsync()`
- `RefreshAccessTokenAsync()`
- `ValidateAccessTokenAsync()`
- `GetUserInfoAsync()`
- `GetOAuthUriAsync()`
- `HandleOAuthCallbackAsync()`
- `GetAccessTokenLifetimeAsync()`

### Session Data Storage

You can now store additional authentication-related data beyond just access tokens:

```csharp
// Store custom session data (automatically serialized to JSON)
authSession.SetAuthSessionData(new { UserId = "123", Preferences = userPrefs });

// Retrieve session data
var sessionData = authSession.GetAuthSessionData<MySessionData>();
```

Session data is stored in cookies and persists across page reloads, making it perfect for storing user preferences, feature flags, or other auth-related state.

## Error Handling Improvements

### Better Exception Details in Development

When exceptions occur during the initial connection to your Ivy app, you'll now see detailed error information instead of a generic "Not Found" page:

- **Error title**: "Internal Server Error" heading
- **Error message**: The actual exception message explaining what went wrong
- **Stack trace**: Full stack trace for debugging

This makes it much easier to diagnose startup issues and connection errors during development. Previously, exceptions thrown in the AppHub connection handler would display a `NotFoundApp` view, which masked the actual error and made debugging difficult.

## Documentation Improvements

### Widget Properties Table Layout

The widget documentation now displays property setters with improved readability. The "Setters" column in the Properties table has been widened to 40% of the table width, giving more space for method signatures and making the documentation easier to scan:

```csharp
// Example of how this appears in the widget documentation
properties.ToTable()
    .Width(Size.Full())
    .ColumnWidth(p => p.Setters, Size.Fraction(0.4f))
```

This improvement applies to all widget documentation pages throughout the framework docs, making method signatures less cramped and more readable when browsing API references.

### Blade Component Layout Structure

The Blades documentation has been updated to show the proper layout structure for blade components. When using the `UseBlades` hook, wrap the blade view in a horizontal layout with an explicit height to ensure proper rendering:

```csharp
// Proper blade layout structure
return Layout.Horizontal().Height(Size.Units(100))
    | this.UseBlades(() => new RootView(), "Blade Title");
```

For individual blade views, the same pattern applies - wrap your content in a horizontal layout with a defined height:

```csharp
public class NavigationRootView : ViewBase
{
    public override object? Build()
    {
        var blades = this.UseContext<IBladeController>();
        var index = blades.GetIndex(this);

        return Layout.Horizontal().Height(Size.Units(50))
            | (Layout.Vertical()
                | Text.Block($"This is blade level {index}")
                | new Button($"Push Blade {index + 1}", onClick: _ =>
                    blades.Push(this, new NavigationRootView(), $"Level {index + 1}"))
                | (index > 0 ? new Button("Go Back", onClick: _ => blades.Pop()) : null));
    }
}
```

This layout structure ensures that blades render with consistent heights and proper horizontal alignment within the blade container. All blade documentation examples have been updated to reflect this best practice.


### Form Validation Best Practices

The Forms documentation has been updated with comprehensive validation examples and best practices:

**Prefer `[Length]` over `[StringLength]`**: The modern `[Length(min, max)]` attribute is now recommended for string validation instead of the older `[StringLength]` attribute:

```csharp
// Recommended approach
[Length(8, 100, ErrorMessage = "Password must be between 8 and 100 characters")]
public string Password { get; set; } = "";

// Old approach (still works, but Length is preferred)
[StringLength(100, MinimumLength = 8)]
public string Password { get; set; } = "";
```

The updated documentation now includes expanded examples showing:
- How to combine multiple validators on the same field
- Validation for collections and arrays using `[MinLength]` and `[Length]`
- Programmatic validation using `.Required()` and `.Validate()` methods
- Complete examples with custom error messages for all validator types

All validation attributes support custom error messages via the `ErrorMessage` parameter, making it easy to provide user-friendly validation feedback.

### Comprehensive Form Scaffolding Examples

The Form sample app has been enhanced with three tabs showcasing different aspects of form functionality:

**Form Tab**: Shows practical examples of forms in action with multiple model types:
- User registration forms with validation
- Database generator forms with boolean fields
- Size demonstrations (Small, Medium, Large) across all input types

**Scaffolding Tab**: Demonstrates how Ivy automatically generates forms and details from C# models:
- `[Display]` attribute usage (Name, Description, Order, Prompt, GroupName)
- String validation (`[Required]`, `[MinLength]`, `[MaxLength]`, `[StringLength]`, `[Length]`)
- Format validation (`[EmailAddress]`, `[CreditCard]`, `[Url]`, `[Phone]`, `[RegularExpression]`)
- Number validation (`[Range]`)
- Hiding fields with `[ScaffoldColumn(false)]`

**Validation Tab**: Complete form validation example with 20+ fields showing:
- All standard DataAnnotation validators working together
- Custom error messages for every validation type
- Programmatic validation using `.Validate()` for custom business rules
- Multiple validators on single properties
- Collection validation with `[AllowedValues]` and `[MinLength]`

```csharp
// Example from the validation tab showing multiple validators
[Display(Name = "User Name", Description = "Enter your full name", Prompt = "John Doe", Order = 1)]
[Required(ErrorMessage = "Name is required")]
[Length(2, 50, ErrorMessage = "Name must be between 2 and 50 characters")]
public string Name { get; init; } = string.Empty;

// Programmatic validation example
var form = model.ToForm("Submit Registration")
    .Validate<DateTime?>(m => m.BirthDate, birthDate =>
        (birthDate == null || birthDate <= DateTime.Now, "Birth date cannot be in the future"))
    .Validate<string>(m => m.Bio, bio =>
        (string.IsNullOrEmpty(bio) || !bio.Contains("spam"), "Bio cannot contain spam content"));
```

These examples serve as a complete reference for building forms with Ivy, from basic scaffolding to advanced validation patterns.

### Collection Validation with `[AllowedValues]`

Form validation now properly handles the `[AllowedValues]` attribute on collection properties. Each item in a collection is validated individually against the allowed values:

```csharp
public class FormModel
{
    [AllowedValues("Draft", "Published", "Archived")]
    public string[] Statuses { get; set; } = Array.Empty<string>();
}

// Valid: all values are in the allowed list
model.Statuses = new[] { "Draft", "Published" };

// Invalid: "Deleted" is not in the allowed list
model.Statuses = new[] { "Draft", "Deleted" };  // Validation error
```

This fix ensures that validation works consistently whether you're validating a single value or a collection of values. The implementation uses a dedicated `TryCreateCollectionValidator` helper method that checks each item in the collection against the allowed values, providing clear error messages when validation fails.

### Improved Email Validation

Email validation has been enhanced to be more accurate and catch malformed email addresses. The validation now:

- Uses `MailAddress` instead of `EmailAddressAttribute` for more robust validation
- Verifies that the domain part contains at least one dot (e.g., `user@example.com`)

This prevents invalid emails like `user@localhost` or `user@domain` from passing validation, ensuring that only properly formatted email addresses with valid domains are accepted in your forms.

### Validation Error Tooltip Fix

The validation error icon (red info icon) in form inputs now properly displays tooltips when you hover over it. Previously, the tooltip might not appear due to pointer event handling issues. This fix applies to all text input variants:

- Default text inputs
- Password inputs
- Search inputs
- Textarea inputs

When a form field has a validation error, you can now reliably hover over the error icon to see the detailed error message, improving the form validation user experience.

### Apps and the `[App]` Attribute

New comprehensive documentation explains how to create and configure apps using the `[App]` attribute. This is essential for understanding how Ivy applications are structured and how routing works.

The `[App]` attribute transforms a standard C# class into a discoverable, routable application component:

```csharp
using Ivy.Apps;

[App(
    title: "Product Catalog",
    icon: Icons.ShoppingBag,
    searchHints: ["store", "items", "inventory"]
)]
public class ProductsApp : ViewBase
{
    // Your app implementation
}
```

**Key capabilities:**

- **Automatic Route Generation**: Ivy uses your namespace structure to generate clean URLs. The framework looks for the `Apps` segment in your namespace, and anything after it becomes the route path
- **Navigation Integration**: Apps automatically appear in navigation menus with their configured icon and title
- **Search Integration**: Use `searchHints` to make your app discoverable via the Command Palette (Cmd/Ctrl+K)
- **Flexible Configuration**: Control visibility, ordering, icons, descriptions, and more

**Available parameters:**

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `id` | `string?` | `null` | Unique identifier for the app. If omitted, generated from class name and namespace |
| `title` | `string?` | `null` | Display name in navigation and window titles. Defaults to readable class name |
| `icon` | `Icons` | `Icons.None` | Icon from the `Icons` enum for navigation and search |
| `description` | `string?` | `null` | Brief description shown in tooltips or app listings |
| `path` | `string[]?` | `null` | Explicitly defines the navigation path, overriding automatic generation |
| `isVisible` | `bool` | `true` | Control whether the app appears in navigation menus |
| `order` | `int` | `0` | Sort order within navigation groups (lower = first) |
| `groupExpanded` | `bool` | `false` | Whether the navigation group is expanded by default |
| `searchHints` | `string[]?` | `null` | Keywords for Command Palette discoverability |

**Route generation examples:**

| Class Name | Full Namespace | Generated Route | URL |
|-----------|----------------|-----------------|-----|
| `DashboardApp` | `MyProject.Apps` | `dashboard-app` | `/dashboard-app` |
| `UserProfile` | `MyProject.Apps.Settings` | `settings/user-profile` | `/settings/user-profile` |
| `AuditLog` | `MyProject.Apps.Admin.Logs` | `admin/logs/audit-log` | `/admin/logs/audit-log` |

Routes are automatically generated from namespaces using kebab-case conversion. You can override this using the `id` or `path` parameters, though following conventions is recommended for consistency.

**Best practices:**

- Suffix app classes with `App` (e.g., `ProductsApp`) for consistency
- Use `searchHints` with synonyms to improve discoverability
- Organize related apps with namespaces to create structured navigation hierarchies

### Stepper Widget Documentation

The `Stepper` widget now has comprehensive documentation with interactive examples. The Stepper displays a horizontal sequence of steps, perfect for wizards, multi-step forms, and sequential workflows:

```csharp
// Basic stepper with three steps
new Stepper(
    null,
    1,  // Currently on step 2 (zero-based index)
    new StepperItem("1", null, "Step 1", "First step"),
    new StepperItem("2", null, "Step 2", "Second step"),
    new StepperItem("3", null, "Step 3", "Third step")
)
```

**Key features:**
- Visual progress indicators showing current, completed, and upcoming steps
- Optional click handlers to allow navigation between steps
- `AllowSelectForward()` extension to enable clicking on future steps
- Support for dynamic icons (e.g., checkmarks on completed steps)
- Each step can include a symbol, icon, label, and description

### Chat Widget with Streaming Responses

The `Chat` widget documentation now includes an interactive example showing how to implement real-time streaming responses, perfect for AI assistants and chatbots that generate responses incrementally:

```csharp
// Stream a response word by word
var messages = UseState(ImmutableArray.Create<ChatMessage>(
    new ChatMessage(ChatSender.Assistant, "I'm a streaming assistant!")
));

void OnSendMessage(Event<Chat, string> @event)
{
    // Add user message immediately
    var messagesWithUser = messages.Value.Add(
        new ChatMessage(ChatSender.User, @event.Value)
    );
    messages.Set(messagesWithUser);

    // Show loading state
    var assistantMessageIndex = messagesWithUser.Length;
    var messagesWithLoading = messagesWithUser.Add(
        new ChatMessage(ChatSender.Assistant, new ChatStatus("Thinking..."))
    );
    messages.Set(messagesWithLoading);

    // Stream response word by word
    _ = Task.Run(async () =>
    {
        await Task.Delay(2000);

        var words = new[] { "I'm", "processing", "your", "message..." };
        var collectedWords = new List<string>();

        foreach (var word in words)
        {
            collectedWords.Add(word);
            var text = string.Join(" ", collectedWords);

            var all = messages.Value.ToList();
            all[assistantMessageIndex] = new ChatMessage(ChatSender.Assistant, text);
            messages.Set(all.ToImmutableArray());

            await Task.Delay(300);
        }
    });
}
```

**Key features:**
- Shows immediate loading state with `ChatStatus` while generating responses
- Updates messages incrementally as new content arrives
- Perfect for integrating with streaming APIs (OpenAI, Claude, etc.)
- Provides real-time feedback to users instead of waiting for complete responses

### Sheet Widget with Triggers Pattern

The Sheet documentation now includes a complete example showing how to integrate sheets with stateful widgets using the `UseTrigger` hook. This pattern is perfect for creating edit dialogs, detail views, and modal forms:

```csharp
// Create a triggered sheet that opens with parameters
var (sheetView, showEdit) = this.UseTrigger((IState<bool> isOpen, string taskId) =>
    new TaskFormSheet(isOpen, taskId, tasks, client));

// Trigger the sheet from anywhere in your view
.HandleClick(() => showEdit(task.Id))

// Include the sheet view in your layout
return new Fragment()
    | mainContent
    | sheetView;
```

The example demonstrates a Kanban board where clicking any card opens a sheet with an editable form. This pattern shows how to:
- Pass parameters (like task IDs) when opening sheets
- Integrate forms with validation inside sheets
- Update parent state from within a sheet
- Handle save/cancel actions with proper loading states

## Layout Improvements

### Default Padding in Chrome-Free Mode

When you disable the application chrome (`chrome=false` URL parameter), Ivy now automatically adds sensible padding to your content. This ensures your app looks polished even without the standard navigation and sidebar chrome:

```csharp
// When users access your app with ?chrome=false
// Content is automatically wrapped with padding and proper overflow handling
// No code changes needed - it just works!
```

This is particularly useful for embedding Ivy apps in iframes or displaying them as standalone pages where you want to hide the framework chrome but still maintain proper spacing and scrolling behavior.

### Customizable Sidebar Width

You can now customize the width of sidebars when using `ChromeSettings`. The default sidebar width remains 16rem (256px), but you can now adjust it to fit your application's design:

```csharp
ChromeSettings.Default()
    .Width(Size.Rem(20))  // Wider sidebar

// Or use pixels
ChromeSettings.Default()
    .Width(Size.Px(300))
```

This applies to the main application sidebar and ensures consistent spacing and layout across your app. The sidebar smoothly transitions when toggled open/closed, maintaining the custom width you specify. The sidebar toggle button has been repositioned to the main content area for better visual consistency, and all sidebar menu items are now left-aligned for improved readability.

## New Widgets

### AI Button Variant

The `Button` widget now includes an eye-catching AI variant with an animated rainbow gradient border, perfect for highlighting AI-powered features and actions:

```csharp
// Basic AI button
new Button("Generate with AI", onClick, variant: ButtonVariant.Ai)

// With icon
new Button("Ask AI", onClick, variant: ButtonVariant.Ai)
    .Icon(Icons.Sparkles)

// Different sizes
new Button("Small AI", onClick, variant: ButtonVariant.Ai).Small()
new Button("Large AI", onClick, variant: ButtonVariant.Ai).Large()

// Fully rounded corners
new Button("AI Assistant", onClick, variant: ButtonVariant.Ai)
    .BorderRadius(BorderRadius.Full)
```

**Key features:**
- Animated rotating RGB gradient border that continuously cycles through rainbow colors
- Smooth 3-second rotation animation with blur effect for a modern look
- Works with all button sizes (small, default, large, icon)
- Supports custom border radius settings
- Maintains accessibility features like disabled states and tooltips

The AI variant uses a subtle background with the animated gradient providing visual interest, making it perfect for:
- AI chat interfaces and assistants
- Content generation features
- Smart suggestions and recommendations
- Any AI-powered functionality you want to highlight

The animation is performant and runs smoothly without impacting application performance. Use the `.Ai()` extension method as a shorthand for setting the AI variant.

### Expandable Widget

The new `Expandable` widget allows you to create collapsible content sections, perfect for FAQs, settings panels, and information hierarchies:

```csharp
// Basic expandable with title and content
new Expandable("Click to expand",
    "This is the hidden content that appears when you expand the widget.")

// Rich content with nested widgets
new Expandable("Advanced Options",
    Layout.Vertical().Gap(2)
        | Text.H3("Configuration")
        | new Badge("Beta").Secondary()
        | Text.Small("These settings are experimental"))

// Nested expandables for hierarchical content
new Expandable("Main Section",
    Layout.Vertical().Gap(2)
        | Text.Block("Main content here")
        | new Expandable("Sub-section 1", "Nested content 1")
        | new Expandable("Sub-section 2", "Nested content 2"))

// Control initial state and disable when needed
new Expandable("Default Open", "Content").Open()
new Expandable("Disabled", "Content").Disabled()
```

**Key features:**
- Smooth expand/collapse animations
- Support for any widget as content
- Nesting support for hierarchical information
- Control over initial open/closed state
- Can be disabled to prevent user interaction

### AsyncSelectInput Widget

The new `AsyncSelectInput` widget provides a powerful async dropdown with search, perfect for loading options from APIs or databases:

```csharp
// Load options asynchronously based on search term
var selectedUser = UseState<User?>(null);

new AsyncSelectInput<User>(
    selectedUser,
    searchTerm => LoadUsersFromApi(searchTerm),
    user => user.Name  // Display format
)
    .Placeholder("Search users...")
    .Label("Select User")

// Example API function
async Task<User[]> LoadUsersFromApi(string searchTerm)
{
    var response = await httpClient.GetAsync($"/api/users?search={searchTerm}");
    return await response.Content.ReadFromJsonAsync<User[]>();
}
```

**Key features:**
- Async option loading - fetch data from APIs as users type
- Built-in search/filter functionality
- Loading states handled automatically
- Works seamlessly with Ivy's state management
- Supports custom display formatters
- Perfect for large datasets that can't be loaded upfront

**Layout improvements:** The internal layout has been refactored to use flexbox positioning for icons and validation indicators, providing better alignment and more predictable visual appearance across different screen sizes and scales.

**Icon positioning fix:** The validation error icon and chevron icon now render with proper alignment using a simplified flexbox layout. The previous absolute positioning approach has been replaced with a cleaner flex-based layout that eliminates hardcoded positioning values and provides consistent icon placement across all scale variants (Small, Medium, Large).

## Widget Updates

### Kanban Widget Layout Improvement

The `Kanban` widget container no longer includes a top margin, allowing for better integration with parent layouts and more predictable positioning:

```csharp
// Kanban now sits flush with its container's top edge
new Kanban(columns)
    .Height(Size.Full())
```

This removes the previously hard-coded 3rem (48px) top margin, giving you more control over spacing and alignment when embedding Kanban boards in your layouts. If you need spacing, you can now explicitly add it through layout containers or card padding.

### Code Widget XML Language Support

The `Code` widget now supports XML syntax highlighting, making it easier to display configuration files, markup, and structured documents:

```csharp
// Display XML with proper syntax highlighting
new Code("""
    <!-- Project configuration file -->
    <Project Sdk="Microsoft.NET.Sdk">
      <PropertyGroup>
        <OutputType>Exe</OutputType>
        <TargetFramework>net9.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
      </PropertyGroup>
    </Project>
    """, Languages.Xml)
```

This adds to the existing language support which includes C#, JavaScript, TypeScript, Python, SQL, CSS, JSON, DBML, Markdown, and plain text.

### Callout Text Color Consistency

The `Callout` widget now uses consistent text colors across all variants (Info, Success, Warning, Error). Previously, each variant used its own foreground color (cyan-foreground, emerald-foreground, etc.), which could create readability issues. Now all callouts use the standard `text-foreground` color, ensuring better contrast and readability while maintaining the distinctive colored backgrounds and borders for each variant.

This change makes callout text more consistent with the rest of your UI and improves accessibility, especially in dark mode.

### Chart Tooltip Improvements

Chart tooltips now render correctly without text being cut off at container boundaries. The tooltip positioning has been improved by using `appendToBody: true`, which ensures tooltips can extend beyond chart containers and remain fully visible.

**What changed:**
- All chart types (Area, Bar, Line, Pie, etc.) now use consistent tooltip rendering
- Tooltips append to the document body instead of their parent container
- Prevents clipping issues when charts are in scrollable or overflow-hidden containers

This fix applies to all chart types and makes data visualization tooltips more reliable and user-friendly, particularly for charts displayed in cards, sheets, or other constrained layouts.

### DropdownMenu Click Handling Fix

Dropdown menus now properly stop click event propagation, preventing clicks inside dropdown content from triggering actions on parent elements. This fixes issues where selecting a dropdown item could unintentionally trigger click handlers on containers or parent widgets.

### Number Input Width Control

The `NumberInput` widget now supports width customization through the `Width()` extension method, giving you better control over form layouts:

```csharp
// Set a specific width for number inputs
new NumberInput(value, min: 0, max: 100)
    .Width("200px")

// Use with forms for precise layout control
new Form(model)
    | new NumberInput(model, m => m.Quantity)
        .Width("150px")
        .Label("Quantity")
```

This works for both default and slider variants of the number input, making it easier to create consistent, well-aligned forms.

### Details Widget Size Control

The `Details` widget now supports three size variants through the scale API, giving you better control over detail view density and typography:

```csharp
// Small size - compact for dense information displays
record.ToDetails().Small()

// Medium size - the default, balanced appearance
record.ToDetails()  // or .Medium()

// Large size - spacious for important information
record.ToDetails().Large()
```

**What changed:**
- Added `.Small()`, `.Medium()`, and `.Large()` extension methods to `DetailsBuilder`
- All detail labels and values automatically scale their padding and text size
- The scale setting cascades to nested details through context
- Improved alignment by removing extra left padding on labels and values

This is perfect for creating information hierarchies where primary details are larger and more prominent while nested or secondary details can be smaller:

```csharp
var record = new
{
    FirstName = "John",
    LastName = "Doe",
    Age = 30,
    Address = new
    {
        Street = "123 Elm St",
        City = "Springfield",
        State = "IL",
        Zip = "62701"
    }.ToDetails().Small()  // Nested details shown smaller
};

new Card(record.ToDetails().Large())  // Parent details shown larger
```

Each size variant adjusts:
- **Padding**: Small (8px), Medium (12px), Large (16px)
- **Text size**: Small (12px), Medium (14px), Large (16px)
- **Label weight**: Bold across all sizes for clear visual hierarchy

The Details sample app now includes a side-by-side visual comparison of all three size variants (Small, Medium, Large), making it easy to see the differences in padding and text sizing across the scale options.

### Enhanced Card Header Layout

The `Card` widget now supports more flexible header layouts with proper alignment between titles, icons, and other content. Headers can now use full layout widgets for better control:

```csharp
// New approach: Use Layout.Horizontal() for full control
new Card(
    content: "Card content here",
    header: Layout.Horizontal().Align(Align.Center)
            | Text.H4("Card Title").WithLayout().Grow()
            | Icons.Info.ToIcon().Color(Colors.Black)
)
```

**What changed:**
- Card headers can now contain any widget, not just simple text
- Icons and titles automatically align horizontally with proper spacing
- The `Description()` extension method now accepts any object (not just strings), giving you more flexibility with formatting
- The `.Title()` extension method now accepts any object (not just strings), allowing rich widgets as titles

**For metric cards and dashboards**, this makes it much easier to create professional-looking cards with properly aligned icons:

```csharp
new Card(
    content: Layout.Horizontal().Align(Align.Left).Gap(2)
             | Text.Large("$84,250")
             | Icons.TrendingUp.ToIcon().Color(Colors.Emerald)
             | Text.Small("21%").Color(Colors.Emerald),
    footer: new Progress(21).Goal("800000"),
    header: Layout.Horizontal().Align(Align.Center)
            | Text.H4("Total Sales").WithLayout().Grow()
            | Icons.DollarSign.ToIcon().Color(Colors.Black)
)
```

**Simplified Header API**: The `.Header()` method now provides a more streamlined way to set both title and description:

```csharp
// Combined title and description
new Card("Content here")
    .Header("Card Title", "This is the description")

// With icon
new Card("Content here")
    .Header(Text.H4("Card Title"), Text.Muted("Description"), Icons.Info)
```

**Improved Card Scaling**: Card headers and content now have better text size consistency across the Small, Medium, and Large scale variants:

```csharp
// Small cards: Header text is 14px, content is 14px
new Card("Content").Header("Title").Small()

// Medium cards: Header text is 16px
new Card("Content").Header("Title").Medium()

// Large cards: Header text is 18px
new Card("Content").Header("Title").Large()
```

Headers now normalize heading styles (`h1`-`h6`) to match the card's scale size without extra margin, creating cleaner, more predictable layouts. This ensures that metric cards and dashboard widgets have consistent typography regardless of which heading level you use in headers.

**Better Icon Alignment**: Card headers now properly align icons with titles vertically, ensuring consistent spacing and professional appearance:

```csharp
// Icons now align perfectly with titles at all card scales
new Card(
    content: "Card content",
    header: Layout.Horizontal().Align(Align.Center)
            | Text.H4("Card Title").WithLayout().Grow()
            | Icons.Info.ToIcon().Color(Colors.Black)
).Small()
```

The header layout now uses flexbox alignment to ensure icons sit perfectly centered relative to the title text, regardless of card scale (Small, Medium, or Large). This fix particularly improves the appearance of metric cards and dashboard widgets where icons are commonly used alongside titles.

### Simplified `Box` Widget Defaults

The `Box` widget now has cleaner, more neutral defaults that work better as a general-purpose container:

```csharp
// Old defaults
new Box()  // Had Color.Primary, BorderThickness=2, Center aligned

// New defaults
new Box()  // No color, BorderThickness=1, TopLeft aligned
```

**What changed:**
- **Color**: Changed from `Colors.Primary` to `null` (no background color by default)
- **Border thickness**: Reduced from `2` to `1` pixel for a subtler appearance
- **Content alignment**: Changed from `Align.Center` to `Align.TopLeft` for more predictable layout behavior

The `Box.Plain()` extension method has been removed since the default `Box` now provides the clean, minimal styling that `Plain()` used to offer.

**Migration:**
If you were relying on the old defaults, you can explicitly set the properties:

```csharp
// To get the old Primary-colored, centered box:
new Box()
    .Color(Colors.Primary)
    .BorderThickness(2)
    .ContentAlign(Align.Center)
```

### SelectInput Nullable Value Handling

The `SelectInput` widget now properly handles nullable values when cleared. Both the Toggle and Radio variants correctly set empty values to an empty string instead of `undefined`, ensuring consistent behavior and better compatibility with form validation:

```csharp
// When users clear a SelectInput, it now correctly reports an empty value
var selectedOption = UseState<string?>(null);

new SelectInput(selectedOption, options)
    .Variant(SelectInputVariant.Toggle)
// Clearing this input now properly sets the value to empty string
```

This fix ensures that nullable select inputs work correctly with form validation and state management, preventing issues where cleared values might be treated inconsistently.

### Tabs Underline Positioning Fix

The `Tabs` widget's active indicator underline now stays properly within the container bounds. Previously, the underline could extend slightly below the tab container, causing visual alignment issues. The positioning has been corrected to ensure the underline is always contained within the tabs area:

```csharp
// The active tab underline now renders correctly within bounds
new Tabs(selectedTab)
    .Tab("Overview", new OverviewView())
    .Tab("Details", new DetailsView())
    .Tab("Settings", new SettingsView())
```

This provides a cleaner, more polished appearance for tab navigation throughout your application.

### Tabs Visual Styling Improvements

The `Tabs` widget now has refined background colors and hover states for a cleaner, more subtle appearance:

**What changed:**
- Inactive tabs now use the background color instead of muted, creating better visual harmony with the rest of your UI
- Hover effects are more subtle in light mode (10% opacity instead of 20%)
- Active tabs have a cleaner background that blends better with content areas

These adjustments create a more refined, professional look while maintaining clear visual distinction between active and inactive tabs.

### Toggle and DateTimeInput Visual Refinements

Several visual improvements have been made to form input widgets for better consistency:

**Toggle widget:**
- Large variant icon size adjusted from 24px to 20px for better visual balance

**DateTimeInput (Time variant):**
- Improved internal layout structure for better icon and input alignment
- Clock icon now properly aligns with input text at all scales
- Border and focus ring styling is now more consistent with other input widgets

These refinements ensure form inputs have consistent sizing and alignment across all scale variants (Small, Medium, Large).

### EmbedCard Focus Ring Removal

The `EmbedCard` widget no longer shows a green focus ring when keyboard navigating to embedded links and buttons. The focus outline is still present for accessibility (`:focus-outline`), but the colored ring has been removed for a cleaner, more consistent appearance across the application.

This provides better visual consistency while maintaining keyboard navigation support for accessibility.

### TableBuilder Reset Method

`TableBuilder` now includes a `Reset()` method that restores all columns to their initial smart defaults, undoing any customizations you've made:

```csharp
var table = products.ToTable()
    .Remove(x => x.Id)
    .Align(x => x.Price, Align.Left)
    .Header(x => x.Name, "Product Name");

// Later, undo all customizations and restore smart defaults
table.Reset();

// Table is now back to its initial state:
// - All columns visible (including Id)
// - Numeric columns right-aligned
// - Boolean columns center-aligned
// - String columns left-aligned
// - All headers restored to default formatting
```

This is particularly useful when building configurable table views where users can customize and reset their preferences, or when you want to experiment with table configurations and quickly revert changes.

## New Hooks

### `UseRef` Hook for Non-Reactive State

Ivy now includes a `UseRef` hook for storing values that persist across renders without triggering re-renders when they change. This is perfect for storing references, cached values, or internal state that doesn't affect the UI:

```csharp
// Store a value without triggering re-renders
var counterRef = this.UseRef(0);

// Update the value (doesn't cause a re-render)
counterRef.Set(counterRef.Value + 1);

// Access the current value
var currentCount = counterRef.Value;
```

```csharp
// Initialize with a factory function
var expensiveRef = this.UseRef(() => CalculateExpensiveValue());
```

**Key differences from `UseState`:**
- `UseRef` values persist across renders but **don't trigger re-renders** when changed
- `UseState` values trigger re-renders when changed, updating the UI
- Use `UseRef` for internal tracking, timers, previous values, or any state that shouldn't affect rendering

Under the hood, `UseRef` is implemented as `UseState` with `buildOnChange: false`, giving you the persistence of state without the reactive behavior.

### Improved Reliability for `UseAlert` and `UseTrigger`

The `UseAlert` and `UseTrigger` hooks have been refactored internally to be more reliable and consistent. They now use `UseRef` for internal state tracking (instead of `UseState`), which prevents unnecessary re-renders and improves performance. The implementation uses a `FuncView` wrapper with internal state synchronization to ensure the UI updates correctly while keeping external state non-reactive.

This fixes several edge cases where these hooks could behave inconsistently, especially when dealing with rapid state changes or complex component hierarchies. The API remains the same, so no code changes are needed on your end - your alerts and triggers will simply work more reliably.

## Theming & Design System

### Expanded Color Palette

The design system now includes a comprehensive set of neutral and chromatic colors, all with proper foreground color variants for accessible text. These colors are automatically injected as CSS variables and work seamlessly in both light and dark themes.

**Neutral colors available:**
- Black, White, Slate, Gray, Zinc, Neutral, Stone

**Chromatic colors available:**
- Red, Orange, Amber, Yellow, Lime, Green, Emerald, Teal, Cyan, Sky, Blue, Indigo, Violet, Purple, Fuchsia, Pink, Rose

Each color includes both a background variant and a foreground variant (e.g., `--red` and `--red-foreground`), ensuring text remains readable when placed on colored backgrounds. These colors are perfect for:
- Data visualizations and charts
- Status indicators and badges
- Category color-coding
- Accent elements throughout your UI

The colors are automatically available through the theming system and respect both light and dark mode preferences.

## Performance Improvements

### Font Loading Optimization

Ivy now preloads all essential Geist and Geist Mono font weights (Regular, Medium, SemiBold, Bold) in the initial HTML document. This eliminates the font flicker that could occur during page load when the browser discovers fonts late in the rendering process.

**What changed:**
- All primary font files are now preloaded with `<link rel="preload">` in the document head
- Fonts load earlier in the page lifecycle, before the browser needs them for rendering
- Results in smoother initial page loads with no visible font switching

This optimization is completely automatic - no code changes needed in your apps. You'll simply notice that text renders with the correct fonts immediately on page load.

## Platform Updates

### .NET 10 Upgrade

Ivy Framework now targets .NET 10, bringing the latest runtime performance improvements and language features. To upgrade your project:

1. Update your project's target framework:

```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
</PropertyGroup>
```

2. Install the .NET 10 SDK from [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)

All Ivy packages and dependencies have been updated to support .NET 10, including Entity Framework Core, Microsoft Identity libraries, and other core dependencies.

**Microsoft Entra authentication users**: If you're using the Microsoft Entra auth provider, the underlying Microsoft.Identity.Client library has been updated, which may include security fixes and performance improvements. No code changes are required on your part.

## Security Improvements

### Enhanced URL Validation for Embed and VideoPlayer Widgets

Fixed a security vulnerability in the Embed and VideoPlayer widgets where malicious URLs could potentially bypass hostname validation through substring matching or subdomain attacks. The new validation properly validates hostnames to prevent attacks like:

- `https://evil.com/youtube.com` (substring matching - platform name in path)
- `https://youtube.com.evil.com` (subdomain attack - parent domain)

**What changed:**
- Introduced `validateEmbedUrl()` function with explicit hostname mapping for supported platforms
- Validation now uses exact hostname matching or proper subdomain checking
- Only allows HTTP and HTTPS protocols (blocks `javascript:`, `data:`, `file:`, etc.)
- Hostname comparison is case-insensitive
- Comprehensive test suite with 291 test cases covering security scenarios

**Supported platforms validated:**
- YouTube (`youtube.com`, `youtu.be`)
- Twitter/X (`twitter.com`, `x.com`)
- Facebook, Instagram, TikTok
- LinkedIn, Pinterest (`pinterest.com`, `pin.it`)
- GitHub (`github.com`, `gist.github.com`)
- Reddit

This security fix is automatic and requires no code changes in your applications. Your embed and video player widgets are now protected against URL-based injection attacks.

### Enhanced String Escaping in Document Tools

Fixed a security vulnerability in the document copy-to-markdown functionality where incomplete string escaping could potentially allow injection attacks. The fix ensures proper escaping order when processing table cells:

**What changed:**
- Backslashes are now escaped first (converted to `\\`), before pipe characters
- This prevents edge cases where malformed input could bypass escaping logic
- The escaping order is critical: backslashes must be escaped before other special characters to prevent injection

This security fix applies to the DocumentTools widget's table cell processing and ensures that markdown table generation is safe from injection attacks. The improvement is automatic and requires no code changes in your applications.

## Bug Fixes

### Improved App Routing and Default App Selection

The framework's app routing system has been improved to prevent certain system apps from being automatically selected as the default app:

**What changed:**
- Authentication, Chrome, and Error pages are now excluded from auto-default app selection
- Better error handling when no suitable default app is found
- Fixed incorrect widget link in DataTable sample (now correctly points to `/widgets/charts/area-chart`)

This prevents scenarios where users might accidentally land on authentication or error pages when the framework tries to resolve a default app. If no suitable app is available, you'll now get a clear error message: "No serviceable apps are registered on this server."

## Widget Updates

### Badge Icon Improvements

The `Badge` widget now has better icon placement and sizing across all scale variants:

**What changed:**
- Icon sizing now uses consistent proportional scaling (Small: 3 units, Medium: 4 units, Large: 5 units)
- Added automatic gap spacing between badge elements using `gap-1` for better visual separation
- Icons no longer use margin classes (`mr-1`, `ml-1`), improving layout consistency
- Icon sizes are now applied via inline styles for more precise rendering

```csharp
// Icons now have better spacing and sizing at all scales
new Badge("Status").Icon(Icons.Check).Small()    // 3-unit icon
new Badge("Status").Icon(Icons.Check)            // 4-unit icon (default)
new Badge("Status").Icon(Icons.Check).Large()    // 5-unit icon
```

The improved icon handling creates cleaner, more balanced badge appearances, especially when mixing badges of different sizes in the same view.

## Documentation Improvements

### Comprehensive Widget Documentation

This week saw a major documentation effort with new and improved guides for many core widgets. All documentation now includes interactive examples, best practices, and clear usage patterns:

**Newly documented widgets:**
- **Badge**: Complete guide covering all variants, sizes, icons, and common use cases like status indicators and counters
- **Blades**: In-depth documentation on blade navigation patterns, headers, refresh tokens, and error handling
- **Button**: Comprehensive coverage of variants, states, styling options, and URL integration
- **Details**: Complete guide to displaying structured data with custom builders and nested objects
- **DropDownMenu**: Full documentation including positioning, headers, and fluent syntax
- **Expandable**: Guide to creating collapsible content sections with nested structures
- **List**: Documentation covering ListItem configuration, interactive lists, and dynamic content
- **Pagination**: Guide to pagination controls with configuration examples
- **Progress**: Complete documentation with state-bound progress bars and examples
- **Table**: Comprehensive TableBuilder guide with cell builders and configuration options
- **Tooltip**: Documentation covering form validation, icons, and rich content
- **DateTimeInput**: Updated guide for date, datetime, and time pickers
- **Box**: Complete primitive widget documentation with styling and layout examples

All widget documentation pages now feature:
- Clear, concise descriptions
- Interactive demo examples you can try immediately
- Code snippets showing common patterns
- Best practices and usage recommendations
- Links to source code for deeper exploration

These improvements make it easier to discover widget capabilities and learn Ivy by example. The documentation is auto-generated from markdown files and includes live demos that run directly in the docs site.

## Developer Tools

### Enhanced Roslyn Analyzer for Hook Rules

The Ivy.Analyser package now enforces comprehensive Rules of Hooks at compile time, catching common mistakes before your code runs. The analyzer automatically detects any method starting with `Use` followed by an uppercase letter (e.g., `UseState`, `UseCustomHook`).

**New diagnostic rules:**

- **IVYHOOK001** (Error): Hook called outside valid context - Hooks must be called directly in the `Build()` method, not in lambdas, local functions, or other methods
- **IVYHOOK002** (Warning): Hook called conditionally - Hooks cannot be inside `if` statements, ternary operators, or try-catch blocks
- **IVYHOOK003** (Warning): Hook called in loop - Hooks cannot be inside `for`, `foreach`, `while`, or `do-while` loops
- **IVYHOOK004** (Warning): Hook called in switch statement - Hooks cannot be inside switch cases
- **IVYHOOK005** (Warning): Hook not at top of method - All hooks must be called at the very top of the `Build()` method before any other statements

These rules ensure hooks are called in the same order on every render, which is critical for state management to work correctly. The analyzer works automatically in Visual Studio, VS Code, and Rider.

**Example violations caught:**

```csharp
// ❌ IVYHOOK002 - Conditional hook
if (condition) {
    var state = UseState(0); // Warning!
}

// ❌ IVYHOOK003 - Hook in loop
foreach (var item in items) {
    var state = UseState(item); // Warning!
}

// ❌ IVYHOOK005 - Hook not at top
var x = SomeMethod();
var state = UseState(0); // Warning! Hook must come first
```

**Correct usage:**

```csharp
// ✅ All hooks at the top, unconditionally
var state1 = UseState(0);
var state2 = UseState("hello");
UseEffect(() => { });

// Then your logic
if (condition) {
    // Use state values here (not hook calls)
    var value = state1.Value;
}
```

The analyzer has been extensively tested with over 40 test cases covering both valid and invalid hook usage patterns.

**Auto-detection of custom hooks**: The analyzer now automatically detects any method starting with `Use` followed by an uppercase letter (e.g., `UseCustomHook`, `UseMyFeature`) as a hook, without requiring a hardcoded list. This means your custom hooks will be automatically validated without any configuration.

### Widget Tree Debug Logging

For debugging widget tree updates during development, you can now enable detailed logging by setting an environment variable:

```bash
export IVY_DUMP_WIDGET_TREES=1
```

When enabled, widget tree updates will be logged to `dump.ljson` in your working directory. This is helpful for understanding how your UI updates are being processed and for troubleshooting rendering issues.

**Performance tracking**: The debug logs now include elapsed time measurements for each tree update, helping you identify performance bottlenecks and optimize slow-rendering components during development.
